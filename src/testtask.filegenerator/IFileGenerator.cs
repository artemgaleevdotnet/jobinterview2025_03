using System.Numerics;

namespace testtask.filegenerator
{
    public interface IFileGenerator
    {
        public Task GenerateFile(string filename, long fileSizeInBytes, CancellationToken cancellationToken);
    }
}