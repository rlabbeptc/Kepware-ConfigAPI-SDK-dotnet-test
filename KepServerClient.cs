using KepwareSync.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KepwareSync
{
    public class KepServerClient
    {
        private readonly ILogger<KepServerClient> m_logger;
        private readonly HttpClient m_httpClient;

        public KepServerClient(ILogger<KepServerClient> logger)
        {
            m_logger = logger;
            m_httpClient = CreateHttpClient();
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
        
        private HttpClient CreateHttpClient()
        {

            string username = "Administrator";
            string password = "InrayTkeDocker2024!";
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            bool acceptSelfSignedCertificates = true;

            // Create and configure HttpClientHandler to accept self-signed certificates
            var handler = acceptSelfSignedCertificates ? new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            } : new HttpClientHandler();

            // Create and configure HttpClient
            return new HttpClient(handler)
            {
                BaseAddress = new Uri("https://localhost:57512/"),
                Timeout = TimeSpan.FromSeconds(30),
                DefaultRequestHeaders =
                {
                    { "Accept", "application/json" },
                    { "User-Agent", "KepwareSync" },
                    { "Authorization", $"Basic {credentials}" }
                },
            };
        }

        public Task<T?> LoadAsync<T>(BaseEntity? owner = null)
          where T : EntityCollection<DefaultEntity>, new()
         => LoadAsync<T, DefaultEntity>(owner);

        public async Task<T?> LoadAsync<T, K>(BaseEntity? owner = null)
            where T : EntityCollection<K>, new()
            where K : BaseEntity, new()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            var endpoint = ResolveEndpoint<T>(owner);

            m_logger.LogInformation($"Loading {typeof(T).Name} from {endpoint}...");
            var response = await m_httpClient.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                m_logger.LogError($"Failed to load {typeof(T).Name} from {endpoint}: {response.ReasonPhrase}");
                return default;
            }

            var collection = await DeserializeJsonAsync<K>(response);
            if (collection != null)
            {
                // if generic type K implements IHaveOwner
                if (collection.OfType<IHaveOwner>().Any())
                {
                    foreach (var item in collection.OfType<IHaveOwner>())
                    {
                        item.Owner = owner;
                    }
                }

                var resultCollection = new T() { Owner = owner, Items = collection };
                return resultCollection; 
            }
            else
            {
                m_logger.LogError($"Failed to deserialize {typeof(T).Name} from {endpoint}");
                return default;
            }
        }

        protected async Task<List<K>?> DeserializeJsonAsync<K>(HttpResponseMessage httpResponse)
            where K : BaseEntity, new()
        {
            try
            {
                using (var stream = await httpResponse.Content.ReadAsStreamAsync())
                    return await JsonSerializer.DeserializeAsync(stream, KepJsonContext.GetJsonListTypeInfo<K>());
            }
            catch (JsonException ex)
            {
                m_logger.LogError($"JSON Deserialization failed: {ex.Message}");
                return null;
            }
        }

        private string ResolveEndpoint<T>(BaseEntity? owner)
        {
            var endpointTemplate = typeof(T).GetCustomAttributes(typeof(EndpointAttribute), false)
                .OfType<EndpointAttribute>()
                .FirstOrDefault()?.EndpointTemplate;

            if (endpointTemplate == null)
            {
                throw new InvalidOperationException($"No endpoint defined for {GetType().Name}");
            }

            //Regex to find all placeholders in the endpoint template
            var placeholders = Regex.Matches(endpointTemplate, @"\{(.+?)\}")
                .Reverse();

            // owner -> owner.Owner -> owner.Owner.Owner -> ... to replace the placeholders in the endpoint template by reverse order

            foreach (Match placeholder in placeholders)
            {
                var placeholderName = placeholder.Groups[1].Value;

                endpointTemplate = endpointTemplate.Replace(placeholder.Value, owner?.Name);

                if (owner is IHaveOwner ownable && ownable.Owner != null)
                    owner = ownable.Owner;
                else
                    break;
            }


            return endpointTemplate;
        }
    }
}
