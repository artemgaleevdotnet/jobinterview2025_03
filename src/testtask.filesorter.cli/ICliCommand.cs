using System.CommandLine;

public interface ICliCommand
{
    Command InitializeCommand(CancellationToken cancellationToken);
}