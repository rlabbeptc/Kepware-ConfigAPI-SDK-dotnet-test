using KepwareSync.Model;
using KepwareSync.ProjectStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using System;
using System.CommandLine.Invocation;
using System.CommandLine;
using System.Net.Http;
using System.Text;
using System.CommandLine.Builder;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Hosting;
using KepwareSync.Configuration;
using Microsoft.Extensions.Configuration;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using System.Linq;
using KepwareSync.Serializer;

namespace KepwareSync
{
    internal class Program
    {
        static Task Main(string[] args)
        {
            // Binder
            var kepApiBinder = new KepApiOptionsBinder();
            var kepStorageBinder = new KepStorageOptionsBinder();

            // Root Command
            var rootCommand = new RootCommand("KepwareSync CLI Tool");
            kepApiBinder.BindTo(rootCommand);
            kepStorageBinder.BindTo(rootCommand);

            rootCommand.SetHandler(RunRootCommand, kepApiBinder, kepStorageBinder);

            // SyncToDisk Command
            var syncToDiskCommand = new Command("SyncToDisk", "Synchronize data to disk");
            kepApiBinder.BindTo(syncToDiskCommand);
            kepStorageBinder.BindTo(syncToDiskCommand);

            syncToDiskCommand.SetHandler(SyncToDisk, kepApiBinder, kepStorageBinder);

            // SyncFromDisk Command
            var syncFromDiskCommand = new Command("SyncFromDisk", "Synchronize data from disk");
            kepApiBinder.BindTo(syncFromDiskCommand);
            kepStorageBinder.BindTo(syncFromDiskCommand);

            syncFromDiskCommand.SetHandler(SyncFromDisk, kepApiBinder, kepStorageBinder);

            // Commands hinzufügen
            rootCommand.AddCommand(syncToDiskCommand);
            rootCommand.AddCommand(syncFromDiskCommand);

            return rootCommand.InvokeAsync(args);
        }


        private static async Task RunRootCommand(KepApiOptions apiOptions, KepStorageOptions kepStorageOptions)
        {
            var host = await BuildHost(apiOptions, kepStorageOptions);

            await host.RunAsync();
        }

        private static async Task SyncToDisk(KepApiOptions apiOptions, KepStorageOptions kepStorageOptions)
        {
            var host = await BuildHost(apiOptions, kepStorageOptions);

            var kepServerClient = host.Services.GetRequiredService<KepServerClient>();
            var project = await kepServerClient.LoadProject(true);

            var storage = host.Services.GetRequiredService<KepFolderStorage>();

            await storage.ExportProjecAsync(project);

            var syncService = host.Services.GetRequiredService<SyncService>();


            Console.WriteLine("Sync to disk completed.");
        }

        private static async Task SyncFromDisk(KepApiOptions apiOptions, KepStorageOptions kepStorageOptions)
        {
            var host = await BuildHost(apiOptions, kepStorageOptions);

            var storage = host.Services.GetRequiredService<KepFolderStorage>();

            var projectFromDisk = await storage.LoadProject(true);

            var kepServerClient = host.Services.GetRequiredService<KepServerClient>();
            var projectFromApi = await kepServerClient.LoadProject(true);


            var prjCompare = EntityCompare.Compare(projectFromDisk, projectFromApi);
            try
            {
                if (projectFromDisk.Hash != projectFromApi.Hash)
                {
                    //TODO update project
                }

                var channelCompare = await kepServerClient.CompareAndApply<ChannelCollection, Channel>(projectFromDisk.Channels, projectFromApi.Channels);

                foreach (var channel in channelCompare.UnchangedItems.Concat(channelCompare.ChangedItems))
                {
                    var deviceCompare = await kepServerClient.CompareAndApply<DeviceCollection, Device>(channel.Left!.Devices, channel.Right!.Devices, channel.Right);

                    foreach (var device in deviceCompare.UnchangedItems.Concat(deviceCompare.ChangedItems))
                    {
                        var tagCompare = await kepServerClient.CompareAndApply<DeviceTagCollection, Tag>(device.Left!.Tags, device.Right!.Tags, device.Right);
                        var tagGroupCompare = await kepServerClient.CompareAndApply<DeviceTagGroupCollection, DeviceTagGroup>(device.Left!.TagGroups, device.Right!.TagGroups, device.Right);

                        foreach (var tagGroup in tagGroupCompare.UnchangedItems.Concat(tagGroupCompare.ChangedItems))
                        {
                            if (tagGroup.Left?.TagGroups != null)
                                await RecusivlyCompareTagGroup(kepServerClient, tagGroup.Left!.TagGroups, tagGroup.Right!.TagGroups, tagGroup.Right);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("Sync from disk completed.");
        }

        private static async Task RecusivlyCompareTagGroup(KepServerClient kepServerClient, DeviceTagGroupCollection left, DeviceTagGroupCollection? right, NamedEntity owner)
        {
            var tagGroupCompare = await kepServerClient.CompareAndApply<DeviceTagGroupCollection, DeviceTagGroup>(left, right, owner);

            foreach (var tagGroup in tagGroupCompare.UnchangedItems.Concat(tagGroupCompare.ChangedItems))
            {
                var tagGroupTagCompare = await kepServerClient.CompareAndApply<DeviceTagGroupTagCollection, Tag>(tagGroup.Left!.Tags, tagGroup.Right!.Tags, tagGroup.Right);

                if (tagGroup.Left!.TagGroups != null)
                    await RecusivlyCompareTagGroup(kepServerClient, tagGroup.Left!.TagGroups, tagGroup.Right!.TagGroups, tagGroup.Right);
            }
        }

        #region BuildHost
        private static Task<IHost> BuildHost(KepApiOptions apiOptions, KepStorageOptions kepStorageOptions)
        {
            var builder = Host.CreateApplicationBuilder();
            var configuration = builder.Configuration;

            configuration
                 .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            apiOptions.UserName ??= configuration["KepApi:Username"];
            apiOptions.Password ??= configuration["KepApi:Password"];
            apiOptions.Host ??= configuration["KepApi:Host"];

            kepStorageOptions.Directory ??= configuration["KepStorage:Directory"];
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
            kepStorageOptions.PersistDefaultValue ??= configuration.GetValue<bool>("KepStorage:PersistDefaultValue");
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code

            if (apiOptions.UserName == null || apiOptions.Password == null || apiOptions.Host == null)
            {
                throw new InvalidOperationException("Missing configuration for KepApiOptions");
            }

            builder.Services.AddLogging();
            builder.Services.AddSerilog((services, loggerConfiguration) =>
            {
                loggerConfiguration
                    .ReadFrom.Configuration(builder.Configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Console();
            });
            builder.Services.AddSingleton<YamlSerializer>();
            builder.Services.AddSingleton<CsvTagSerializer>();
            builder.Services.AddSingleton(apiOptions);
            builder.Services.AddSingleton(kepStorageOptions);
            builder.Services.AddSingleton<IProjectStorage, JsonFlatFileProjectStorage>();
            builder.Services.AddSingleton<SyncService>();
            builder.Services.AddSingleton<KepFolderStorage>();
            builder.Services.AddHttpClient<KepServerClient>(client =>
            {
                client.BaseAddress = new Uri(apiOptions.Host);
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", "KepwareSync");

                string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{apiOptions.UserName}:{apiOptions.Password}"));
                client.DefaultRequestHeaders.Add("Authorization", $"Basic {credentials}");
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                return handler;
            })
            .AddPolicyHandler(GetRetryPolicy());

            builder.Services.AddSingleton<GitClient>();
            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();

            return Task.FromResult(host);
        }
        #endregion

        #region retryPolicy
        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        if (context.TryGetValue("Logger", out var objLogger) && objLogger is ILogger logger)
                            logger.LogWarning($"Delaying for {timespan.TotalSeconds} seconds, then making retry {retryAttempt}");
                    });
        }
        #endregion
    }
}
