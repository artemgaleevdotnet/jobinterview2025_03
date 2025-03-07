using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

internal class Program
{
    /// <summary>
    /// testtask-test sort --input-file-path D:\bigfile.txt --output-file-path D:\sortedFile.txt
    /// testtask-test generate --file-path D:\bigfile.txt --size 1000000
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    private static Task Main(string[] args)
    {
        Console.WriteLine("testtask test task.");

        var rootCommand = new RootCommand
        {
            Name = "testtask-test",
            Description = "Large File Utility"
        };

        var commands = DependecyInjection.ConfigureApplication().BuildServiceProvider().GetServices<ICliCommand>();

        foreach (var command in commands)
        {
            rootCommand.AddCommand(command.InitializeCommand(default));
        }

        return rootCommand.InvokeAsync(args);
    }
}
