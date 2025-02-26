using Kepware.Api.Model;
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

namespace Kepware.Api
{
    public partial class KepwareApiClient
    {
        public Task<AdminSettings?> GetAdminSettingsAsync(CancellationToken cancellationToken = default)
        {
            return LoadEntityAsync<AdminSettings>(name: null, cancellationToken);
        }

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

                HttpContent httpContent = new StringContent(JsonSerializer.Serialize(diff, KepJsonContext.Default.DictionaryStringJsonElement), Encoding.UTF8, "application/json");
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
    }
}
