using KepwareSync.Model;
using KepwareSync.ProjectStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Net.Http;
using System.Text;

namespace KepwareSync
{
    internal class Program
    {
        static async Task Main(string[] args)
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

            var kepServerClient = host.Services.GetRequiredService<KepServerClient>();

            var channels = await kepServerClient.LoadProject();

            var syncService = host.Services.GetRequiredService<SyncService>();

            // Example usage
            syncService.NotifyChange(new ChangeEvent
            {
                Source = ChangeSource.LocalFile,
            });

            await host.RunAsync();
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
