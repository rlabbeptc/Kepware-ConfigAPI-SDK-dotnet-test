using Kepware.Api.Model;
using Kepware.Api.Model.Services;
using Kepware.Api.Serializer;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Kepware.Api
{
    public partial class KepwareApiClient
    {
        private const string ENDPOINT_REINITIALIZE_RUNTIME = "/config/v1/project/services/ReinitializeRuntime";


        /// <summary>
        /// Initiates the Reinitialize Runtime service.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a KepServerJobPromise.</returns>
        public Task<KepServerJobPromise> ReinitializeRuntimeAsync(CancellationToken cancellationToken = default)
            => ReinitializeRuntimeAsync(TimeSpan.FromSeconds(30), cancellationToken);

        /// <summary>
        /// Initiates the Reinitialize Runtime service.
        /// </summary>
        /// <param name="timeToLive">The job's desired Time to Live (timeout).</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a KepServerJobPromise.</returns>
        public async Task<KepServerJobPromise> ReinitializeRuntimeAsync(TimeSpan timeToLive, CancellationToken cancellationToken = default)
        {
            if (timeToLive.TotalSeconds < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(timeToLive), "Time to live must be at least 1 second");
            }
            else if (timeToLive.TotalSeconds > 300)
            {
                throw new ArgumentOutOfRangeException(nameof(timeToLive), "Time to live must be at most 300 seconds");
            }

            var request = new ReinitializeRuntimeRequest { TimeToLiveSeconds = (int)timeToLive.TotalSeconds };
            HttpContent httpContent = new StringContent(JsonSerializer.Serialize(request, KepJsonContext.Default.ReinitializeRuntimeRequest), Encoding.UTF8, "application/json");

            var response = await m_httpClient.PutAsync(ENDPOINT_REINITIALIZE_RUNTIME, httpContent, cancellationToken).ConfigureAwait(false);


            var message = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                var jobResponse = JsonSerializer.Deserialize<JobResponseMessage>(message, KepJsonContext.Default.JobResponseMessage);

                if (jobResponse != null)
                {
                    return new KepServerJobPromise(ENDPOINT_REINITIALIZE_RUNTIME, timeToLive, jobResponse, m_httpClient);
                }
                else
                {
                    return new KepServerJobPromise(ENDPOINT_REINITIALIZE_RUNTIME, timeToLive, (ApiResponseCode)(int)response.StatusCode, "Failed to deserialize response message");
                }
            }
            catch (JsonException jex)
            {
                return new KepServerJobPromise(ENDPOINT_REINITIALIZE_RUNTIME, timeToLive, (ApiResponseCode)(int)response.StatusCode, jex.Message);
            }
        }
    }
}
