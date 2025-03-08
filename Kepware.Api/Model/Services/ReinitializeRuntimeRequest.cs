using System.Text.Json.Serialization;

namespace Kepware.Api.Model.Services
{
    public class ReinitializeRuntimeRequest
    {
        [JsonPropertyName(Properties.Name)]
        public string Name { get; set; } = "ReinitializeRuntime";

        [JsonPropertyName(Properties.Job.TimeToLive)]
        public int TimeToLiveSeconds { get; set; } = 30;
    }
}
