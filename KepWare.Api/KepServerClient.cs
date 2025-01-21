using Kepware.Api.Model;
using Kepware.Api.Serializer;
using Kepware.Api.Util;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Kepware.Api
{
    public partial class KepServerClient
    {
        private const string ENDPOINT_STATUS = "/config/v1/status";
        private const string ENDPOINT_ABOUT = "/config/v1/about";
        private const string ENDPONT_FULL_PROJECT = "/config/v1/project?content=serialize";

        private readonly ILogger<KepServerClient> m_logger;
        private readonly HttpClient m_httpClient;
        private readonly Regex m_pathplaceHolderRegex = EndpointPlaceholderRegex();
        private bool? m_blnIsConnected = null;

        public KepServerClient(ILogger<KepServerClient> logger, HttpClient httpClient)
        {
            m_logger = logger;
            m_httpClient = httpClient;
        }

        public async Task<bool> TestConnectionAsync()
        {
            bool blnIsConnected = false;
            try
            {
                if (m_blnIsConnected == null) // first time after connection change
                {
                    m_logger.LogInformation("Connecting to {BaseAddress}...", m_httpClient.BaseAddress);
                }
                var response = await m_httpClient.GetAsync(ENDPOINT_STATUS);

                if (response.IsSuccessStatusCode)
                {
                    var status = await JsonSerializer.DeserializeAsync(await response.Content.ReadAsStreamAsync(), KepJsonContext.Default.ListApiStatus);
                    if (status?.FirstOrDefault()?.Healthy == true)
                    {
                        blnIsConnected = true;
                    }
                }

                if (m_blnIsConnected == null) // first time after connection change
                {
                    if (!blnIsConnected)
                    {
                        m_logger.LogWarning("Failed to connect to {BaseAddress}, Reason: {ReasonPhrase}", m_httpClient.BaseAddress, response.ReasonPhrase);
                    }
                    else
                    {
                        var prodInfo = await GetProductInfoAsync();
                        m_logger.LogInformation("Successfully connected to {ProductName} {ProductVersion} on {BaseAddress}", prodInfo?.ProductName, prodInfo?.ProductVersion, m_httpClient.BaseAddress);
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                if (m_blnIsConnected == null) // first time after connection change
                    m_logger.LogWarning("Failed to connect to {BaseAddress}: {Message}", m_httpClient.BaseAddress, httpEx.Message);
            }
            m_blnIsConnected = blnIsConnected;
            return blnIsConnected;
        }

        public async Task<ProductInfo?> GetProductInfoAsync()
        {
            try
            {
                var response = await m_httpClient.GetAsync(ENDPOINT_ABOUT);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var prodInfo = JsonSerializer.Deserialize(content, KepJsonContext.Default.ProductInfo);

                    m_blnIsConnected = true;
                    return prodInfo;
                }
                else
                {
                    m_logger.LogWarning("Failed to get product info from endpoint {Endpoint}, Reason: {ReasonPhrase}", "/config/v1/about", response.ReasonPhrase);
                }
            }
            catch (HttpRequestException httpEx)
            {
                m_logger.LogWarning(httpEx, "Failed to connect to {BaseAddress}", m_httpClient.BaseAddress);
                m_blnIsConnected = null;
            }

            return null;
        }

        /// <summary>
        /// Compares two collections of entities and applies the changes to the target collection.
        /// Left should represent the source and Right should represent the API (target).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <param name="sourceCollection"></param>
        /// <param name="apiCollection"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        public async Task<EntityCompare.CollectionResultBucket<T, K>> CompareAndApply<T, K>(T? sourceCollection, T? apiCollection, NamedEntity? owner = null)
          where T : EntityCollection<K>
          where K : NamedEntity, new()
        {
            var compareResult = EntityCompare.Compare<T, K>(sourceCollection, apiCollection);

            /// This are the items that are in the API but not in the source
            /// --> we need to delete them
            await DeleteItemsAsync<T, K>(compareResult.ItemsOnlyInRight.Select(i => i.Right!).ToList(), owner);

            /// This are the items both in the API and the source
            /// --> we need to update them
            await UpdateItemsAsync<T, K>(compareResult.ChangedItems.Select(i => (i.Left!, i.Right)).ToList(), owner);

            /// This are the items that are in the source but not in the API
            /// --> we need to insert them
            await InsertItemsAsync<T, K>(compareResult.ItemsOnlyInLeft.Select(i => i.Left!).ToList(), owner: owner);

            return compareResult;
        }


        public async Task<bool> UpdateItemAsync<T>(T item, T? oldItem = default)
           where T : NamedEntity, new()
        {
            try
            {
                var endpoint = ResolveEndpoint<T>(oldItem ?? item);

                m_logger.LogInformation("Updating {TypeName} on {Endpoint}...", typeof(T).Name, endpoint);

                var currentEntity = await LoadEntityAsync<T>(oldItem ?? item);
                item.ProjectId = currentEntity?.ProjectId;

                HttpContent httpContent = new StringContent(JsonSerializer.Serialize(item, KepJsonContext.GetJsonTypeInfo<T>()), Encoding.UTF8, "application/json");
                var response = await m_httpClient.PutAsync(endpoint, httpContent);
                if (!response.IsSuccessStatusCode)
                {
                    var message = await response.Content.ReadAsStringAsync();
                    m_logger.LogError("Failed to update {TypeName} from {Endpoint}: {ReasonPhrase}\n{Message}", typeof(T).Name, endpoint, response.ReasonPhrase, message);
                }
                else
                {
                    return true;
                }
            }
            catch (HttpRequestException httpEx)
            {
                m_logger.LogWarning(httpEx, "Failed to connect to {BaseAddress}", m_httpClient.BaseAddress);
                m_blnIsConnected = null;
            }
            return false;
        }

        public Task UpdateItemAsync<T, K>(K item, K? oldItem = default, NamedEntity? owner = null)
            where T : EntityCollection<K>
          where K : NamedEntity, new()
            => UpdateItemsAsync<T, K>([(item, oldItem)], owner);
        public async Task UpdateItemsAsync<T, K>(List<(K item, K? oldItem)> items, NamedEntity? owner = null)
          where T : EntityCollection<K>
          where K : NamedEntity, new()
        {
            if (items.Count == 0)
                return;

            try
            {
                var collectionEndpoint = ResolveEndpoint<T>(owner).TrimEnd('/');
                foreach (var pair in items)
                {
                    var endpoint = $"{collectionEndpoint}/{Uri.EscapeDataString(pair.oldItem!.Name)}";
                    var currentEntity = await LoadEntityAsync<K>(endpoint, owner);
                    if (currentEntity == null)
                    {
                        m_logger.LogError("Failed to load {TypeName} from {Endpoint}", typeof(K).Name, endpoint);
                    }
                    else
                    {
                        pair.item.ProjectId = currentEntity.ProjectId;
                        var diff = pair.item.GetUpdateDiff(currentEntity);

                        m_logger.LogInformation("Updating {TypeName} on {Endpoint}, values {Diff}", typeof(T).Name, endpoint, diff);

                        HttpContent httpContent = new StringContent(JsonSerializer.Serialize(diff, KepJsonContext.Default.DictionaryStringJsonElement), Encoding.UTF8, "application/json");
                        var response = await m_httpClient.PutAsync(endpoint, httpContent);
                        if (!response.IsSuccessStatusCode)
                        {
                            var message = await response.Content.ReadAsStringAsync();
                            m_logger.LogError("Failed to update {TypeName} from {Endpoint}: {ReasonPhrase}\n{Message}", typeof(T).Name, endpoint, response.ReasonPhrase, message);
                        }
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                m_logger.LogWarning(httpEx, "Failed to connect to {BaseAddress}", m_httpClient.BaseAddress);
                m_blnIsConnected = null;
            }
        }

        public Task InsertItemAsync<T, K>(K item, NamedEntity? owner = null)
          where T : EntityCollection<K>
          where K : NamedEntity, new()
            => InsertItemsAsync<T, K>([item], owner: owner);

        public async Task InsertItemsAsync<T, K>(List<K> items, int pageSize = 10, NamedEntity? owner = null)
         where T : EntityCollection<K>
         where K : NamedEntity, new()
        {
            if (items.Count == 0)
                return;

            try
            {
                var endpoint = ResolveEndpoint<T>(owner);
                var totalPageCount = (int)Math.Ceiling((double)items.Count / pageSize);
                for (int i = 0; i < totalPageCount; i++)
                {
                    var pageItems = items.Skip(i * pageSize).Take(pageSize).ToList();
                    m_logger.LogInformation("Inserting {NumItems} {TypeName} on {Endpoint} in batch {BatchNr} of {TotalBatches} ...", pageItems.Count, typeof(K).Name, endpoint, i + 1, totalPageCount);

                    var jsonContent = JsonSerializer.Serialize(pageItems, KepJsonContext.GetJsonListTypeInfo<K>());
                    HttpContent httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                    var response = await m_httpClient.PostAsync(endpoint, httpContent);
                    if (!response.IsSuccessStatusCode)
                    {
                        var message = await response.Content.ReadAsStringAsync();
                        m_logger.LogError("Failed to insert {TypeName} from {Endpoint}: {ReasonPhrase}\n{Message}", typeof(T).Name, endpoint, response.ReasonPhrase, message);
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.MultiStatus)
                    {
                        // When a POST includes multiple objects, if one or more cannot be processed due to a parsing failure or 
                        // some other non - property validation error, the HTTPS status code 207(Multi - Status) will be returned along
                        // with a JSON object array containing the status for each object in the request.
                        var results = await JsonSerializer.DeserializeAsync(await response.Content.ReadAsStreamAsync(), KepJsonContext.Default.ListApiResult);
                        var failedEntries = results?.Where(r => !r.IsSuccessStatusCode)?.ToList() ?? [];
                        m_logger.LogError("{NumSuccessFull} were successfull, failed to insert {NumFailed} {TypeName} from {Endpoint}: {ReasonPhrase}\nFailed:\n{Message}",
                            (results?.Count ?? 0) - failedEntries.Count, failedEntries.Count, typeof(T).Name, endpoint, response.ReasonPhrase, JsonSerializer.Serialize(failedEntries, KepJsonContext.Default.ListApiResult));
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                m_logger.LogWarning(httpEx, "Failed to connect to {BaseAddress}", m_httpClient.BaseAddress);
                m_blnIsConnected = null;
            }
        }

        public Task DeleteItemAsync<T, K>(K item, NamedEntity? owner = null)
            where T : EntityCollection<K>
            where K : NamedEntity, new()
            => DeleteItemsAsync<T, K>([item], owner);

        public async Task DeleteItemsAsync<T, K>(List<K> items, NamedEntity? owner = null)
            where T : EntityCollection<K>
            where K : NamedEntity, new()
        {
            if (items.Count == 0)
                return;
            try
            {
                var collectionEndpoint = ResolveEndpoint<T>(owner).TrimEnd('/');
                foreach (var item in items)
                {
                    var endpoint = $"{collectionEndpoint}/{Uri.EscapeDataString(item.Name)}";

                    m_logger.LogInformation("Deleting {TypeName} on {Endpoint}...", typeof(K).Name, endpoint);

                    var response = await m_httpClient.DeleteAsync(endpoint);
                    if (!response.IsSuccessStatusCode)
                    {
                        var message = await response.Content.ReadAsStringAsync();
                        m_logger.LogError("Failed to delete {TypeName} from {Endpoint}: {ReasonPhrase}\n{Message}", typeof(T).Name, endpoint, response.ReasonPhrase, message);
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                m_logger.LogWarning(httpEx, "Failed to connect to {BaseAddress}", m_httpClient.BaseAddress);
                m_blnIsConnected = null;
            }
        }


        public async Task<Project> LoadProject(bool blnLoadFullProject = false)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            var productInfo = await GetProductInfoAsync();

            if (blnLoadFullProject && productInfo?.SupportsJsonProjectLoadService == true)
            {
                try
                {
                    var response = await m_httpClient.GetAsync(ENDPONT_FULL_PROJECT);
                    if (response.IsSuccessStatusCode)
                    {
                        var prjRoot = await JsonSerializer.DeserializeAsync(await response.Content.ReadAsStreamAsync(), KepJsonContext.Default.JsonProjectRoot);

                        if (prjRoot?.Project != null)
                        {
                            if (prjRoot.Project.Channels != null)
                                foreach (var channel in prjRoot.Project.Channels)
                                {
                                    if (channel.Devices != null)
                                        foreach (var device in channel.Devices)
                                        {
                                            device.Owner = channel;

                                            if (device.Tags != null)
                                                foreach (var tag in device.Tags)
                                                    tag.Owner = device;

                                            if (device.TagGroups != null)
                                                SetOwnerRecursive(device.TagGroups, device);
                                        }
                                }

                            m_logger.LogInformation("Loaded project via JsonProjectLoad Service in {ElapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);
                            return prjRoot.Project;
                        }
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    m_logger.LogWarning(httpEx, "Failed to connect to {BaseAddress}", m_httpClient.BaseAddress);
                    m_blnIsConnected = null;
                }

                m_logger.LogWarning("Failed to load project");
                return new Project();
            }
            else
            {
                var project = await LoadEntityAsync<Project>();

                if (project == null)
                {
                    m_logger.LogWarning("Failed to load project");
                    project = new Project();
                }
                else if (blnLoadFullProject)
                {
                    project.Channels = await LoadCollectionAsync<ChannelCollection, Channel>();

                    if (project.Channels != null)
                    {
                        int totalChannelCount = project.Channels.Count;
                        int loadedChannelCount = 0;
                        await Task.WhenAll(project.Channels.Select(async channel =>
                        {
                            channel.Devices = await LoadCollectionAsync<DeviceCollection, Device>(channel);

                            if (channel.Devices != null)
                            {
                                await Task.WhenAll(channel.Devices.Select(async device =>
                                {
                                    device.Tags = await LoadCollectionAsync<DeviceTagCollection, Tag>(device);
                                    device.TagGroups = await LoadCollectionAsync<DeviceTagGroupCollection, DeviceTagGroup>(device);

                                    if (device.TagGroups != null)
                                    {
                                        await LoadTagGroupsRecursiveAsync(device.TagGroups);
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
                }

                return project;
            }
        }

        public Task<T?> LoadEntityAsync<T>(NamedEntity? owner = null, IEnumerable<KeyValuePair<string, string>>? queryParams = null)
          where T : BaseEntity, new()
        {
            var endpoint = ResolveEndpoint<T>(owner);
            if (queryParams != null)
            {
                var queryString = string.Join("&", queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
                endpoint += "?" + queryString;
            }
            return LoadEntityAsync<T>(endpoint, owner);
        }

        private async Task<T?> LoadEntityAsync<T>(string endpoint, NamedEntity? owner = null)
          where T : BaseEntity, new()
        {
            try
            {
                m_logger.LogDebug("Loading {TypeName} from {Endpoint}...", typeof(T).Name, endpoint);

                var response = await m_httpClient.GetAsync(endpoint);
                if (!response.IsSuccessStatusCode)
                {
                    m_logger.LogError("Failed to load {TypeName} from {Endpoint}: {ReasonPhrase}", typeof(T).Name, endpoint, response.ReasonPhrase);
                    return default;
                }

                var entity = await DeserializeJsonAsync<T>(response);
                if (entity is IHaveOwner ownable)
                {
                    ownable.Owner = owner;
                }

                return entity;
            }
            catch (HttpRequestException httpEx)
            {
                m_logger.LogWarning(httpEx, "Failed to connect to {BaseAddress}", m_httpClient.BaseAddress);
                m_blnIsConnected = null;
                return default;
            }

        }


        public Task<T?> LoadCollectionAsync<T>(NamedEntity? owner = null)
          where T : EntityCollection<DefaultEntity>, new()
         => LoadCollectionAsync<T, DefaultEntity>(owner);

        public async Task<T?> LoadCollectionAsync<T, K>(NamedEntity? owner = null)
            where T : EntityCollection<K>, new()
            where K : BaseEntity, new()
        {
            var endpoint = ResolveEndpoint<T>(owner);
            try
            {
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

                    var resultCollection = new T() { Owner = owner };
                    resultCollection.AddRange(collection);
                    return resultCollection;
                }
                else
                {
                    m_logger.LogError("Failed to deserialize {TypeName} from {Endpoint}", typeof(T).Name, endpoint);
                    return default;
                }
            }
            catch (HttpRequestException httpEx)
            {
                m_logger.LogWarning(httpEx, "Failed to connect to {BaseAddress}", m_httpClient.BaseAddress);
                m_blnIsConnected = null;
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
                m_logger.LogError(ex, "JSON Deserialization failed");
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
                m_logger.LogError(ex, "JSON Deserialization failed");
                return null;
            }
        }

        private void SetOwnerRecursive(IEnumerable<DeviceTagGroup> tagGroups, NamedEntity owner)
        {
            foreach (var tagGroup in tagGroups)
            {
                tagGroup.Owner = owner;

                if (tagGroup.Tags != null)
                    foreach (var tag in tagGroup.Tags)
                        tag.Owner = tagGroup;

                if (tagGroup.TagGroups != null)
                    SetOwnerRecursive(tagGroup.TagGroups, tagGroup);
            }
        }

        private async Task LoadTagGroupsRecursiveAsync(IEnumerable<DeviceTagGroup> tagGroups)
        {
            foreach (var tagGroup in tagGroups)
            {
                // Lade die TagGroups der aktuellen TagGroup
                tagGroup.TagGroups = await LoadCollectionAsync<DeviceTagGroupCollection, DeviceTagGroup>(tagGroup);
                tagGroup.Tags = await LoadCollectionAsync<DeviceTagGroupTagCollection, Tag>(tagGroup);

                // Rekursiver Aufruf für die geladenen TagGroups
                if (tagGroup.TagGroups != null && tagGroup.TagGroups.Count > 0)
                {
                    await LoadTagGroupsRecursiveAsync(tagGroup.TagGroups);
                }
            }
        }
        private string ReplacePlaceholders(string template, NamedEntity? owner)
        {
            var placeholders = m_pathplaceHolderRegex.Matches(template).Reverse();

            foreach (Match placeholder in placeholders)
            {
                var placeholderName = placeholder.Groups[1].Value;

                string? placeholderValue = owner?.Name;
                if (!string.IsNullOrEmpty(placeholderValue))
                {
                    template = template.Replace(placeholder.Value, Uri.EscapeDataString(placeholderValue));

                    if (owner is IHaveOwner ownable && ownable.Owner != null)
                        owner = ownable.Owner;
                    else
                        break;
                }
                else
                {
                    throw new InvalidOperationException($"Placeholder '{placeholderName}' in template '{template}' could not be resolved.");
                }
            }

            return template;
        }

        private string ResolveRecursiveEndpoint<T>(RecursiveEndpointAttribute attribute, NamedEntity? owner)
        {
            LinkedList<string> recursivePath = new LinkedList<string>();
            while (owner != null && attribute.RecursiveOwnerType == owner?.GetType())
            {
                var currentEndpointPart = ReplacePlaceholders(attribute.RecursiveEnd, owner);
                recursivePath.AddFirst(currentEndpointPart);

                if (owner is IHaveOwner ownable && ownable.Owner is NamedEntity nextOwner)
                    owner = nextOwner;
                else
                    owner = null;
            }

            // Combine with the base endpoint template
            var baseEndpoint = ReplacePlaceholders(attribute.EndpointTemplate, owner);

            return baseEndpoint + string.Concat(recursivePath);
        }

        private string ResolveEndpoint<T>(NamedEntity? owner)
        {
            var endpointTemplateAttribute = typeof(T).GetCustomAttributes(typeof(EndpointAttribute), false)
                .OfType<EndpointAttribute>()
                .FirstOrDefault();

            if (endpointTemplateAttribute == null)
            {
                throw new InvalidOperationException($"No endpoint defined for {typeof(T).Name}");
            }

            if (endpointTemplateAttribute is RecursiveEndpointAttribute recursiveEndpointAttribute && recursiveEndpointAttribute.RecursiveOwnerType == owner?.GetType())
            {
                return ResolveRecursiveEndpoint<T>(recursiveEndpointAttribute, owner) + endpointTemplateAttribute.Suffix;
            }

            return ReplacePlaceholders(endpointTemplateAttribute.EndpointTemplate, owner) + endpointTemplateAttribute.Suffix;
        }


        [GeneratedRegex(@"\{(.+?)\}", RegexOptions.Compiled)]
        private static partial Regex EndpointPlaceholderRegex();
    }
}
