using testtask.sorter;
using System.CommandLine;

internal sealed class SortCommand : Command, ICliCommand
{
    private readonly IFileSorter _sorter;

    public SortCommand(IFileSorter sorter) 
        : base("sort", "Sort a file by lines")
    {
        _sorter = sorter;
    }

    public Command InitializeCommand(CancellationToken cancellationToken)
    {
        var inputFileOption = new Option<string>(
            "--input-file-path",
            "The path of the input file to sort");
        var outputFileOption = new Option<string>(
            "--output-file-path",
            "The path where the sorted file will be saved");

        AddOption(inputFileOption);
        AddOption(outputFileOption);

        this.SetHandler((inputFile, outputFile) => SortHandler(inputFile, outputFile, cancellationToken), inputFileOption, outputFileOption);

        return this;
    }

    private Task SortHandler(string inputFilePath, string outputFilePath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(inputFilePath))
        {
            Console.WriteLine("Input file path can not be empty.");
            return Task.CompletedTask;
        }
            
        if(string.IsNullOrEmpty(outputFilePath))
        {
            Console.WriteLine("Output file path can not be empty.");
            return Task.CompletedTask;
        }
                
        return _sorter.SortFile(inputFilePath, outputFilePath, cancellationToken);        
    }
}
