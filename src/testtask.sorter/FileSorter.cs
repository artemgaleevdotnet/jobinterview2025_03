using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace testtask.sorter
{
    internal sealed class FileSorter : IFileSorter
    {
        private readonly int _maxLineSize;
        private readonly int _chunkSize;
        private readonly ParsedStringComparer _comparator;
        private readonly int _readBufferSize;
        private readonly int _writeBufferSize;
        private int TempFileIndex = 0;

        private readonly ConcurrentQueue<long> _blocks;
        private readonly ConcurrentBag<string> _tempFiles;
        private readonly FileSorterSettings _options;

        public FileSorter(IOptions<FileSorterSettings> options)
        {
            _options = options.Value;
            var availableMemoryCounter = new PerformanceCounter("Memory", "Available MBytes");
            long availableMemoryBytes = (long)availableMemoryCounter.NextValue() * 1024 * 1024;

            long usableMemory = (long)(availableMemoryBytes * _options.MemoryUsageFactor);
            long memoryPerThread = usableMemory / Environment.ProcessorCount;

            _maxLineSize = _options.MaxStringSize;
            _chunkSize = (int)(memoryPerThread / _maxLineSize);

            _readBufferSize = _options.ReadBufferSize;
            _writeBufferSize = _options.WriteBufferSize;

            _comparator = new ParsedStringComparer();
            _blocks = new ConcurrentQueue<long>();
            _tempFiles = [];
        }

        public async Task SortFile(string inputFilePath, string outputFilePath, CancellationToken cancellationToken)
        {
            GetFileBlocks(inputFilePath);

            var tasks = Enumerable.Range(0, Environment.ProcessorCount).Select(index => SplitAndSortBlocks(inputFilePath, cancellationToken));

            await Task.WhenAll(tasks);

            MergeTempFiles(outputFilePath);

            var deleteTasks = _tempFiles.Select(tempFile => Task.Run(() => File.Delete(tempFile), cancellationToken));

            await Task.WhenAll(deleteTasks);
        }

        private void GetFileBlocks(string inputFilePath)
        {
            long position = 0;
            long currentOffset = 0;
            int lineCount = 0;

            var encoding = Encoding.UTF8;

            using var stream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, _readBufferSize, useAsync: true);
            using var reader = new StreamReader(stream, encoding, true, _readBufferSize);

            string? line;

            while ((line = reader.ReadLine()) != null)
            {
                currentOffset += encoding.GetByteCount(line) + encoding.GetByteCount(Environment.NewLine);

                lineCount++;

                if (lineCount >= _chunkSize || reader.EndOfStream)
                {
                    _blocks.Enqueue(position);
                    position = currentOffset;
                    lineCount = 0;
                }
            }
        }

        private async Task SplitAndSortBlocks(string inputFilePath, CancellationToken cancellationToken)
        {
            while (_blocks.TryDequeue(out var startPosition))
            {
                using var fileStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, _readBufferSize, useAsync: true);
                fileStream.Seek(startPosition, SeekOrigin.Begin);
                using var inputFile = new StreamReader(fileStream, Encoding.UTF8, true, _readBufferSize);

                List<ParsedString> chunk = new(_chunkSize);
                int readIndex = 0;

                while (!inputFile.EndOfStream && readIndex < _chunkSize)
                {
                    string? line = await inputFile.ReadLineAsync(cancellationToken);

                    if (line == null) continue;

                    var parsedLine = new ParsedString(line, _options.Delimiter);

                    chunk.Add(parsedLine);

                    readIndex++;
                }

                chunk.Sort(_comparator);

                Interlocked.Increment(ref TempFileIndex);
                string tempFilePath = $"temp_{TempFileIndex}.txt";

                await WriteChunkToFile(chunk, tempFilePath, cancellationToken);

                chunk.Clear();

                _tempFiles.Add(tempFilePath);
            }
        }

        private async Task WriteChunkToFile(IReadOnlyCollection<ParsedString> chunk, string filePath, CancellationToken cancellationToken)
        {
            var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, _readBufferSize, useAsync: true);
            using var outputFile = new StreamWriter(stream);

            StringBuilder buffer = new(_writeBufferSize + _maxLineSize);

            foreach (var line in chunk)
            {
                buffer
                    .Append(line.NumberPart)
                    .Append(_options.Delimiter)
                    .Append(line.StringPart)
                    .Append(Environment.NewLine);

                if (buffer.Length >= _writeBufferSize)
                {
                    await outputFile.WriteAsync(buffer, cancellationToken);

                    buffer.Clear();
                }
            }

            if (buffer.Length > 0)
            {
                await outputFile.WriteAsync(buffer, cancellationToken);
                buffer.Clear();
            }
        }

        private void MergeTempFiles(string outputFilePath)
        {
            using var outputFile = new StreamWriter(outputFilePath);

            var readers = _tempFiles.Select(tempFile => new StreamReader(tempFile)).ToArray();

            var minHeap = new PriorityQueue<(ParsedString parsedString, int fileIndex), ParsedString>(_comparator);

            for (int i = 0; i < readers.Length; i++)
            {
                if (!readers[i].EndOfStream)
                {
                    var parsedString = new ParsedString(readers[i].ReadLine(), _options.Delimiter);

                    minHeap.Enqueue((parsedString, i), parsedString);
                }
            }

            StringBuilder buffer = new(_writeBufferSize + _maxLineSize);

            while (minHeap.Count > 0)
            {
                var minEntry = minHeap.Dequeue();

                buffer
                    .Append(minEntry.parsedString.NumberPart)
                    .Append(_options.Delimiter)
                    .Append(minEntry.parsedString.StringPart)
                    .Append(Environment.NewLine);

                if (buffer.Length >= _writeBufferSize)
                {
                    outputFile.Write(buffer);
                    buffer.Clear();
                }

                if (!readers[minEntry.fileIndex].EndOfStream)
                {
                    var parsedString = new ParsedString(readers[minEntry.fileIndex].ReadLine(), _options.Delimiter);

                    minHeap.Enqueue((parsedString, minEntry.fileIndex), parsedString);
                }
            }

            if (buffer.Length > 0)
            {
                outputFile.Write(buffer);
                buffer.Clear();
            }

            foreach (var reader in readers)
            {
                reader.Close();
            }
        }
    }
}