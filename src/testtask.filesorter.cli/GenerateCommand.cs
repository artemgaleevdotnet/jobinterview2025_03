using testtask.filegenerator;
using System.CommandLine;
using System.Numerics;

internal sealed class GenerateCommand : Command, ICliCommand
{
    private readonly IFileGenerator _fileGenerator;

    public GenerateCommand(IFileGenerator fileGenerator) 
        : base("generate", "Generate a file with random content")
    {
        _fileGenerator = fileGenerator;
    }

    public Command InitializeCommand(CancellationToken cancellationToken)
    {
        var filePathOption = new Option<string>(
            "--file-path",
            "The path where the file will be generated");
        var sizeOption = new Option<string>(
            "--size",
            "The size of the file in bytes");

        AddOption(filePathOption);
        AddOption(sizeOption);

        this.SetHandler((filePath, size)=> GenerateHandler(filePath, size, cancellationToken), filePathOption, sizeOption);

        return this;
    }

    private Task GenerateHandler(string filePath, string size, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            Console.WriteLine("File path can not be empty.");

            return Task.CompletedTask;
        }

        if (!long.TryParse(size, out var sizeInt) && sizeInt <= 0)
        {
            Console.WriteLine("File size can not be less or equals 0.");

            return Task.CompletedTask;
        }

        return _fileGenerator.GenerateFile(filePath, sizeInt, cancellationToken);
    }
}
