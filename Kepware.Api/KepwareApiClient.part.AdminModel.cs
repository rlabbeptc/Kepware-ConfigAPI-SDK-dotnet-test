using Kepware.Api.Model;
using Kepware.Api.Model.Admin;
using Kepware.Api.Serializer;
using Kepware.Api.Util;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Kepware.Api
{
    public partial class KepwareApiClient
    {
        #region AdminSettings

        /// <summary>
        /// Retrieves the current administrator settings asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A token that can be used to request cancellation of the operation.</param>
        /// <returns>The current <see cref="AdminSettings"/> or null if retrieval fails.</returns>
        public Task<AdminSettings?> GetAdminSettingsAsync(CancellationToken cancellationToken = default)
        {
            return LoadEntityAsync<AdminSettings>(name: null, cancellationToken);
        }

        /// <summary>
        /// Updates the administrator settings with the specified values.
        /// </summary>
        /// <param name="settings">The new administrator settings to apply.</param>
        /// <param name="cancellationToken">A token that can be used to request cancellation of the operation.</param>
        /// <returns>True if the settings were successfully updated; otherwise, false.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the current settings cannot be retrieved.</exception>
        public async Task<bool> SetAdminSettingsAsync(AdminSettings settings, CancellationToken cancellationToken = default)
        {
            try
            {
                var currentSettings = await GetAdminSettingsAsync(cancellationToken).ConfigureAwait(false);

                if (currentSettings == null)
                {
                    throw new InvalidOperationException("Failed to retrieve current settings");
                }

                var endpoint = EndpointResolver.ResolveEndpoint<AdminSettings>();
                var diff = settings.GetUpdateDiff(currentSettings);

                m_logger.LogInformation("Updating AdminSettings on {Endpoint}, values {Diff}", endpoint, diff);

                HttpContent httpContent = new StringContent(
                    JsonSerializer.Serialize(diff, KepJsonContext.Default.DictionaryStringJsonElement),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await m_httpClient.PutAsync(endpoint, httpContent, cancellationToken).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    var message = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    m_logger.LogError("Failed to update AdminSettings from {Endpoint}: {ReasonPhrase}\n{Message}", endpoint, response.ReasonPhrase, message);
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

        #endregion

        #region UaEndpoint

        /// <summary>
        /// Retrieves an OPC UA endpoint configuration asynchronously.
        /// </summary>
        /// <param name="name">The name of the OPC UA endpoint.</param>
        /// <param name="cancellationToken">A token that can be used to request cancellation of the operation.</param>
        /// <returns>The <see cref="UaEndpoint"/> configuration, or null if not found.</returns>
        public Task<UaEndpoint?> GetUaEndpointAsync(string name, CancellationToken cancellationToken = default)
        {
            return LoadEntityAsync<UaEndpoint>(name, cancellationToken);
        }

        /// <summary>
        /// Creates a new OPC UA endpoint or updates an existing one.
        /// </summary>
        /// <param name="endpoint">The <see cref="UaEndpoint"/> to create or update.</param>
        /// <param name="cancellationToken">A token that can be used to request cancellation of the operation.</param>
        /// <returns>True if the operation succeeds; otherwise, false.</returns>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="endpoint"/> has no name specified.</exception>
        public async Task<bool> CreateOrUpdateUaEndpointAsync(UaEndpoint endpoint, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(endpoint.Name))
                throw new ArgumentException("Name is required", nameof(endpoint));

            try
            {
                var endpointUrl = EndpointResolver.ResolveEndpoint<UaEndpoint>([endpoint.Name]);
                var currentEndpoint = await LoadEntityByEndpointAsync<UaEndpoint>(endpointUrl, cancellationToken);

                if (currentEndpoint == null)
                {
                    return await InsertItemAsync<UaEndpointCollection, UaEndpoint>(endpoint, cancellationToken: cancellationToken);
                }
                else
                {
                    return await UpdateItemAsync(endpoint: endpointUrl, item: endpoint, currentEntity: currentEndpoint, cancellationToken: cancellationToken);
                }
            }
            catch (HttpRequestException httpEx)
            {
                m_logger.LogWarning(httpEx, "Failed to connect to {BaseAddress}", m_httpClient.BaseAddress);
                m_blnIsConnected = null;
            }

            return false;
        }

        /// <summary>
        /// Deletes an OPC UA endpoint configuration asynchronously.
        /// </summary>
        /// <param name="name">The name of the endpoint to delete.</param>
        /// <param name="cancellationToken">A token that can be used to request cancellation of the operation.</param>
        /// <returns>True if the endpoint was successfully deleted; otherwise, false.</returns>
        public Task<bool> DeleteUaEndpointAsync(string name, CancellationToken cancellationToken = default)
            => DeleteItemAsync<UaEndpoint>(name, cancellationToken);

        #endregion
    }
}
