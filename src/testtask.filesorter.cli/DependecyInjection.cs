using testtask.filegenerator;
using testtask.sorter;
using Microsoft.Extensions.DependencyInjection;

public static class DependecyInjection
{
    public static IServiceCollection ConfigureApplication()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddFileGenerator();
        serviceCollection.AddFileSorter();

        serviceCollection.AddSingleton<ICliCommand, GenerateCommand>();
        serviceCollection.AddSingleton<ICliCommand, SortCommand>();

        return serviceCollection;
    }
}
