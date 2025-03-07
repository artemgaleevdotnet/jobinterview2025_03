using Microsoft.Extensions.Options;
using System.Text;

namespace testtask.filegenerator
{
    internal sealed class FileGenerator(IOptions<FileGenereatorSettings> options) : IFileGenerator
    {
        private readonly FileGenereatorSettings _options = options.Value;
        private readonly StringBuilder buffer = new(options.Value.MaxStringSize);

        public Task GenerateFile(string filePath, long fileSizeInBytes, CancellationToken cancellationToken)
        {
            Random random = new();

            long bytesWritten = 0;
            var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 8192);
            using StreamWriter writer = new(stream);

            while (bytesWritten < fileSizeInBytes)
            {
                long remainingBytes = fileSizeInBytes - bytesWritten;
                long stringLength = Math.Min(remainingBytes, random.Next(_options.Delimiter.Length + 2, _options.MaxStringSize));

                GenerateRandomString(random, stringLength);

                /*
                    !!! There are no another operations except writing to file, so sync approach is preferable.
                    In case we have another threads and/or IO operations, async approach would be better:

                    async writer.WriteAsync(buffer, cancellationToken);
                 */
                writer.Write(buffer);                
                bytesWritten += buffer.Length;
            }

            return Task.CompletedTask;
        }
        
        private void GenerateRandomString(Random random, long byteCount)
        {
            buffer.Clear();

            buffer.Append(byteCount);
            buffer.Append(_options.Delimiter);

            while (buffer.Length + Environment.NewLine.Length < byteCount)
            {
                buffer.Append(_options.Chars[random.Next(_options.Chars.Length)]);
            }

            buffer.Append(Environment.NewLine);
        }
    }
}