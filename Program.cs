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

            var kepSyncBinder = new KepSyncOptionsBinder();

            // Root Command
            var rootCommand = new RootCommand("KepwareSync CLI Tool");
            kepApiBinder.BindTo(rootCommand);
            kepStorageBinder.BindTo(rootCommand);
            kepSyncBinder.BindTo(rootCommand);

            rootCommand.SetHandler(RunRootCommand, kepApiBinder, kepStorageBinder, kepSyncBinder);

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


        private static async Task RunRootCommand(KepApiOptions apiOptions, KepStorageOptions kepStorageOptions, KepSyncOptions syncOption)
        {
            var builder = ConfigureHost(apiOptions, kepStorageOptions, syncOption);

            builder.Services.AddHostedService<SyncService>();

            var host = builder.Build();

            await host.RunAsync();
        }

        private static SyncService CreateSyncService(KepApiOptions apiOptions, KepStorageOptions kepStorageOptions, KepSyncOptions? syncOptions = null)
        {
            var builder = ConfigureHost(apiOptions, kepStorageOptions, syncOptions);
            builder.Services.AddSingleton<SyncService>();
            var host = builder.Build();
            return host.Services.GetRequiredService<SyncService>();
        }

        private static async Task SyncToDisk(KepApiOptions apiOptions, KepStorageOptions kepStorageOptions)
        {
            var syncService = CreateSyncService(apiOptions, kepStorageOptions);
            await syncService.SyncFromKepServerAsync();

            Console.WriteLine("Sync to disk completed.");
        }

        private static async Task SyncFromDisk(KepApiOptions apiOptions, KepStorageOptions kepStorageOptions)
        {
            var syncService = CreateSyncService(apiOptions, kepStorageOptions);
            await syncService.SyncFromLocalFileAsync();

            Console.WriteLine("Sync from disk completed.");
        }


        #region BuildHost
        private static HostApplicationBuilder ConfigureHost(KepApiOptions apiOptions, KepStorageOptions kepStorageOptions, KepSyncOptions? syncOptions = null)
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

            builder.Services.AddSingleton(syncOptions ?? new KepSyncOptions());
            builder.Services.AddSingleton(apiOptions);
            builder.Services.AddSingleton(kepStorageOptions);
            builder.Services.AddSingleton<YamlSerializer>();
            builder.Services.AddSingleton<CsvTagSerializer>();
            builder.Services.AddSingleton<IProjectStorage, KepFolderStorage>();
            builder.Services.AddHttpClient<KepServerClient>(client =>
            {
                client.BaseAddress = new Uri(apiOptions.Host);
                client.Timeout = TimeSpan.FromSeconds(apiOptions.TimeoutInSeconds);
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

            return builder;
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
