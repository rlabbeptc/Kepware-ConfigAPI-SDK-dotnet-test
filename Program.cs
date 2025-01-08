using KepwareSync.ProjectStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KepwareSync
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
             .ConfigureServices((context, services) =>
             {
                 services.AddLogging();
                 services.AddSingleton<IProjectStorage, JsonFlatFileProjectStorage>();
                 services.AddSingleton<SyncService>();
                 services.AddSingleton<KepServerClient>();
                 services.AddSingleton<GitClient>();
             })
             .Build();

            var syncService = host.Services.GetRequiredService<SyncService>();

            // Example usage
            syncService.NotifyChange(new ChangeEvent
            {
                Source = ChangeSource.LocalFile,
            });

            await host.RunAsync();
        }
    }
}
