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

namespace KepwareSync
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await BuildCommandLine()
                .UseHost(remainingArgs => Host.CreateApplicationBuilder(remainingArgs))
            // Command-line options
            var usernameOption = new Option<string>("--username", "Username for KepServer");
            var passwordOption = new Option<string>("--password", "Password for KepServer");
            var hostOption = new Option<string>("--host", "Host URL for KepServer");

            var rootCommand = new RootCommand
            {
                usernameOption,
                passwordOption,
                hostOption
            };
            rootCommand.SetHandler(context =>
            {
                var host = BuildHost(context, args);
                return RunHost(context, host);
            });
            await rootCommand.InvokeAsync(args);
        }

        private static IHost BuildHost(InvocationContext context, string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddLogging();
            builder.Services.AddSingleton<IProjectStorage, JsonFlatFileProjectStorage>();
            builder.Services.AddSingleton<SyncService>();
            builder.Services.AddHttpClient<KepServerClient>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:57512/");
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", "KepwareSync");
                string username = "Administrator";
                string password = "InrayTkeDocker2024!";
                string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
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
            return host;
        }

        private static CommandLineBuilder BuildCommandLine()
        {
            // Define options
            var usernameOption = new Option<string>("--username", "Username for KepServer") { IsRequired = true };
            var passwordOption = new Option<string>("--password", "Password for KepServer") { IsRequired = true };
            var hostOption = new Option<string>("--host", () => "https://localhost:57512/", "Host URL for KepServer");

            // Define commands
            var rootCommand = new RootCommand("Starts the worker process.")
            {
                usernameOption,
                passwordOption,
                hostOption
            };

            rootCommand.Handler = CommandHandler.Create<IHost>(RunWorker);

            var syncToDiskCommand = new Command("SyncToDisk", "Synchronize data to disk.")
            {
                usernameOption,
                passwordOption,
                hostOption
            };
            syncToDiskCommand.Handler = CommandHandler.Create<IHost>(SyncToDisk);

            var syncFromDiskCommand = new Command("SyncFromDisk", "Synchronize data from disk.")
            {
                usernameOption,
                passwordOption,
                hostOption
            };
            syncFromDiskCommand.Handler = CommandHandler.Create<IHost>(SyncFromDisk);

            rootCommand.AddCommand(syncToDiskCommand);
            rootCommand.AddCommand(syncFromDiskCommand);

            return new CommandLineBuilder(rootCommand);
        }

        private static Task RunWorker(IHost host)
        {
            // This will start the Worker service
            return host.RunAsync();
        }

        private static async Task SyncToDisk(IHost host)
        {
            var kepServerClient = host.Services.GetRequiredService<KepServerClient>();

            var channels = await kepServerClient.LoadProject();

            var syncService = host.Services.GetRequiredService<SyncService>();



            Console.WriteLine("Sync to disk completed.");
        }

        private static void SyncFromDisk(IHost host)
        {
            var syncService = host.Services.GetRequiredService<SyncService>();
            
            Console.WriteLine("Sync from disk completed.");
        }


        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        var logger = context.GetLogger();
                        logger?.LogWarning($"Delaying for {timespan.TotalSeconds} seconds, then making retry {retryAttempt}");
                    });
        }
    }

    public static class PollyContextExtensions
    {
        public static ILogger? GetLogger(this Context context)
        {
            if (context.TryGetValue("Logger", out var logger))
            {
                return logger as ILogger;
            }
            return null;
        }
    }
}
