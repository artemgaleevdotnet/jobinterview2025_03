using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

var config = ManualConfig.Create(DefaultConfig.Instance);
config.AddDiagnoser(MemoryDiagnoser.Default);
config.AddDiagnoser(ThreadingDiagnoser.Default);
var summary = BenchmarkRunner.Run<CliBenchmarkTests>();

[SimpleJob(RunStrategy.ColdStart, iterationCount: 1, launchCount: 7)]
[MaxColumn, MeanColumn, MedianColumn, MemoryDiagnoser, ThreadingDiagnoser]
public class CliBenchmarkTests
{
    private string[] generateFileArgs = ["generate", "--file-path", "testfile.txt", "--size", "100000000"];
    private string[] sortFileArgs = ["sort", "--input-file-path", "testfile.txt", "--output-file-path", "sortedfile.txt"];

    private RootCommand InitializeRootCommand()
    {
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
        return rootCommand;
    }

    [Benchmark]
    public async Task BenchmarkGenerateFile()
    {
        var rootCommand = InitializeRootCommand();
        await rootCommand.InvokeAsync(generateFileArgs);
    }

    [Benchmark]
    public async Task BenchmarkSortFile()
    {
        var rootCommand = InitializeRootCommand();
        await rootCommand.InvokeAsync(sortFileArgs);
    }
}