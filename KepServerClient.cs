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
            var project = await LoadEntityAsync<Project>();

            if (project == null)
            {
                m_logger.LogWarning("Failed to load project");
                return new Project();
            }
            else
            {
                project.Channels = await LoadCollectionAsync<ChannelCollection, Channel>();

                if (blnDeep && project.Channels != null)
                {
                    int totalChannelCount = project.Channels.Items.Count;
                    int loadedChannelCount = 0;
                    await Task.WhenAll(project.Channels.Select(async channel =>
                    {
                        channel.Devices = await LoadCollectionAsync<DeviceCollection, Device>(channel);

                        if (channel.Devices != null)
                        {
                            await Task.WhenAll(channel.Devices.Select(async device =>
                            {
                                device.Tags = await LoadAsync<DeviceTagCollection>(device);
                                device.TagGroups = await LoadCollectionAsync<DeviceTagGroupCollection, DeviceTagGroup>(device);

                                if (device.TagGroups != null)
                                {
                                    await Task.WhenAll(device.TagGroups.Select(async tagGroup =>
                                    {
                                        tagGroup.Tags = await LoadAsync<DeviceTagGroupTagCollection>(tagGroup);
                                    }));
                                }
                            }));
                        }
                        // Log information, loaded channel <Name> x of y
                        loadedChannelCount++;
                        if (totalChannelCount == 1)
                        {
                            m_logger.LogInformation("Loaded channel {ChannelName}", channel.Name);
                        }
                        else
                        {
                            m_logger.LogInformation("Loaded channel {ChannelName} {LoadedChannelCount} of {TotalChannelCount}", channel.Name, loadedChannelCount, totalChannelCount);
                        }

                    }));
                }

                m_logger.LogInformation("Loaded project in {ElapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);

                return project;
            }
        }

        public async Task<T?> LoadEntityAsync<T>(NamedEntity? owner = null)
          where T : BaseEntity, new()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            var endpoint = ResolveEndpoint<T>(owner);

            m_logger.LogDebug("Loading {TypeName} from {Endpoint}...", typeof(T).Name, endpoint);
            var response = await m_httpClient.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                m_logger.LogError("Failed to load {TypeName} from {Endpoint}: {ReasonPhrase}", typeof(T).Name, endpoint, response.ReasonPhrase);
                return default;
            }

            var entity = await DeserializeJsonAsync<T>(response);
            if (entity != null && entity is IHaveOwner ownable)
            {
                ownable.Owner = owner;
            }

            return entity;
        }


        public Task<T?> LoadAsync<T>(NamedEntity? owner = null)
          where T : EntityCollection<DefaultEntity>, new()
         => LoadCollectionAsync<T, DefaultEntity>(owner);

        public async Task<T?> LoadCollectionAsync<T, K>(NamedEntity? owner = null)
            where T : EntityCollection<K>, new()
            where K : BaseEntity, new()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            var endpoint = ResolveEndpoint<T>(owner);

            m_logger.LogDebug("Loading {TypeName} from {Endpoint}...", typeof(T).Name, endpoint);
            var response = await m_httpClient.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                m_logger.LogError("Failed to load {TypeName} from {Endpoint}: {ReasonPhrase}", typeof(T).Name, endpoint, response.ReasonPhrase);
                return default;
            }

            var collection = await DeserializeJsonArrayAsync<K>(response);
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
                m_logger.LogError("Failed to deserialize {TypeName} from {Endpoint}", typeof(T).Name, endpoint);
                return default;
            }
        }

        protected async Task<K?> DeserializeJsonAsync<K>(HttpResponseMessage httpResponse)
          where K : BaseEntity, new()
        {
            try
            {
                using (var stream = await httpResponse.Content.ReadAsStreamAsync())
                    return await JsonSerializer.DeserializeAsync(stream, KepJsonContext.GetJsonTypeInfo<K>());
            }
            catch (JsonException ex)
            {
                m_logger.LogError("JSON Deserialization failed: {Message}", ex.Message);
                return null;
            }
        }


        protected async Task<List<K>?> DeserializeJsonArrayAsync<K>(HttpResponseMessage httpResponse)
            where K : BaseEntity, new()
        {
            try
            {
                using (var stream = await httpResponse.Content.ReadAsStreamAsync())
                    return await JsonSerializer.DeserializeAsync(stream, KepJsonContext.GetJsonListTypeInfo<K>());
            }
            catch (JsonException ex)
            {
                m_logger.LogError("JSON Deserialization failed: {Message}", ex.Message);
                return null;
            }
        }

        private string ResolveEndpoint<T>(NamedEntity? owner)
        {
            var endpointTemplate = typeof(T).GetCustomAttributes(typeof(EndpointAttribute), false)
                .OfType<EndpointAttribute>()
                .FirstOrDefault()?.EndpointTemplate;

            if (endpointTemplate == null)
            {
                throw new InvalidOperationException($"No endpoint defined for {typeof(T).Name}");
            }

            // Regex to find all placeholders in the endpoint template
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
