using Microsoft.Extensions.DependencyInjection;

namespace testtask.sorter
{
    public static class DependecyInjection
    {
        public static void AddFileSorter(this IServiceCollection services)
        {
            services.AddSingleton<IFileSorter, FileSorter>();

            _ = services.AddOptions<FileSorterSettings>()
                .Configure((x) =>
                {
                    x.WriteBufferSize = 8 * 1024;
                    x.ReadBufferSize = 64 * 1024;
                    x.MemoryUsageFactor = 0.25;
                    x.MaxStringSize = 100;
                    x.Delimiter = ". ";
                });
        }
    }
}