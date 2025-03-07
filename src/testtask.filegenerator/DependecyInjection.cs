using Microsoft.Extensions.DependencyInjection;

namespace testtask.filegenerator
{
    public static class DependecyInjection
    {
        public static void AddFileGenerator(this IServiceCollection services)
        {
            services.AddSingleton<IFileGenerator, FileGenerator>();

            _ = services.AddOptions<FileGenereatorSettings>()
                .Configure((x) =>
                {
                    x.Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                    x.Delimiter = ". ";
                    x.MaxStringSize = 100;
                });
        }
    }
}