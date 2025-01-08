using KepwareSync.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KepwareSync
{
    public class KepServerClient
    {
        private readonly ILogger<KepServerClient> m_logger;

        public KepServerClient(ILogger<KepServerClient> logger)
        {
            m_logger = logger;
        }

        public async Task<string> GetFullProjectAsync()
        {
            m_logger.LogInformation("Downloading full project from KepServer...");
            // Retrieve full project JSON from KepServer REST API
            return await Task.FromResult("{\"project\":\"example\"}"); // Placeholder
        }

        public async Task UpdateFullProjectAsync(string projectJson)
        {
            m_logger.LogInformation("Uploading full project to KepServer...");
            // Upload full project JSON to KepServer REST API
            await Task.CompletedTask;
        }

        protected async Task<T> LoadAsync<T>(HttpClient client, params (string Key, string Value)[] parentParameters)
            where T : BaseEntity, new()
        {
            var endpoint = ResolveEndpoint<T>(parentParameters);
            var response = await client.GetStringAsync(endpoint);
            return JsonSerializer.Deserialize<T>(response);
        }

        protected async Task SaveAsync<T>(HttpClient client, T entity , params (string Key, string Value)[] parentParameters)
            where T : BaseEntity
        {
            var endpoint = ResolveEndpoint<T>(parentParameters);
            var jsonContent = JsonSerializer.Serialize(entity);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            await client.PutAsync(endpoint, content);
        }

        private string ResolveEndpoint<T>(params (string Key, string Value)[] parentParameters)
            where T : BaseEntity
        {
            var endpointTemplate = typeof(T).GetCustomAttributes(typeof(EndpointAttribute), false)
                .OfType<EndpointAttribute>()
                .FirstOrDefault()?.EndpointTemplate;

            if (endpointTemplate == null)
            {
                throw new InvalidOperationException($"No endpoint defined for {GetType().Name}");
            }

            foreach (var (key, value) in parentParameters)
            {
                endpointTemplate = endpointTemplate.Replace($"{{{key}}}", value);
            }

            return endpointTemplate;
        }
    }
}
