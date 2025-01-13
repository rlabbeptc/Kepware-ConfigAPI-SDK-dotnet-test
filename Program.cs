using KepwareSync.Model;
using KepwareSync.ProjectStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KepwareSync
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
#if DEBUG
                    logging.SetMinimumLevel(LogLevel.Trace);
                    logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);
#else
                        logging.SetMinimumLevel(LogLevel.Information);
#endif
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddLogging();
                    services.AddSingleton<IProjectStorage, JsonFlatFileProjectStorage>();
                    services.AddSingleton<SyncService>();
                    services.AddSingleton<KepServerClient>();
                    services.AddSingleton<GitClient>();
                })
                .Build();

            var kepServerClient = host.Services.GetRequiredService<KepServerClient>();

            var channels = await kepServerClient.LoadAsync<ChannelCollection, Channel>(null);

            if (channels != null)
            {
                foreach (var channel in channels)
                {
                    channel.Devices = await kepServerClient.LoadAsync<DeviceCollection, Device>(channel);

                    if (channel.Devices != null)
                    {
                        foreach (var device in channel.Devices)
                        {
                            device.Tags = await kepServerClient.LoadAsync<DeviceTagCollection>(device);

                            device.TagGroups = await kepServerClient.LoadAsync<DeviceTagGroupCollection, DeviceTagGroup>(device);

                            if (device.TagGroups != null)
                            {
                                foreach (var tagGroup in device.TagGroups)
                                {
                                    tagGroup.Tags = await kepServerClient.LoadAsync<DeviceTagGroupTagCollection>(tagGroup);
                                }
                            }
                        }
                    }
                }
            }

            // store as YAML
            // <basefolder>/<channelName>/channel.yaml
            // <basefolder>/<channelName>/<deviceName>/device.yaml
            // <basefolder>/<channelName>/<deviceName>/<tagGroupName>/tagGroup.yaml

            var baseFolder = "ExportedYaml";
            var yamlExporter = new KepFolderStorage();
            await yamlExporter.ExportChannelsAsYamlAsync(baseFolder, channels);


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
