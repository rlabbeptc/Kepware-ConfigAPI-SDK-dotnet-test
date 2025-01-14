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

        public KepServerClient(ILogger<KepServerClient> logger, HttpClient httpClient)
        {
            m_logger = logger;
            m_httpClient = httpClient;
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

        public async Task<Project> LoadProject(bool blnDeep = true)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            var channels = await LoadAsync<ChannelCollection, Channel>(null);

            if (blnDeep && channels != null)
            {
                int totalChannelCount = channels.Items.Count;
                int loadedChannelCount = 0;
                await Task.WhenAll(channels.Select(async channel =>
                {
                    channel.Devices = await LoadAsync<DeviceCollection, Device>(channel);

                    if (channel.Devices != null)
                    {
                        await Task.WhenAll(channel.Devices.Select(async device =>
                        {
                            device.Tags = await LoadAsync<DeviceTagCollection>(device);
                            device.TagGroups = await LoadAsync<DeviceTagGroupCollection, DeviceTagGroup>(device);

                            if (device.TagGroups != null)
                            {
                                await Task.WhenAll(device.TagGroups.Select(async tagGroup =>
                                {
                                    tagGroup.Tags = await LoadAsync<DeviceTagGroupTagCollection>(tagGroup);
                                }));
                            }
                        }));
                    }
                    //Log information, loaded channel <Name> x of y
                    loadedChannelCount++;
                    m_logger.LogInformation(totalChannelCount == 1 ? $"Loaded channel {channel.Name}" : $"Loaded channel {channel.Name} {loadedChannelCount} of {totalChannelCount}");
                }));
            }

            m_logger.LogInformation($"Loaded project in {stopwatch.ElapsedMilliseconds} ms");

            return new Project { Channels = channels };
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

            m_logger.LogDebug($"Loading {typeof(T).Name} from {endpoint}...");
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

                string? placeholderValue = owner?.Name;
                if (!string.IsNullOrEmpty(placeholderValue))
                {
                    endpointTemplate = endpointTemplate.Replace(placeholder.Value, Uri.EscapeDataString(placeholderValue));

                    if (owner is IHaveOwner ownable && ownable.Owner != null)
                        owner = ownable.Owner;
                    else
                        break;
                }
                else
                {
                    throw new InvalidOperationException($"Placeholder '{placeholderName}' in endpoint template '{endpointTemplate}' could not be resolved.");
                }
            }

            return endpointTemplate;
        }
    }
}
