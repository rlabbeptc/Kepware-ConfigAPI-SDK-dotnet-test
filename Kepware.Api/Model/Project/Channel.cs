using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Kepware.Api.Model
{
    /// <summary>
    /// Represents a channel in the project
    /// </summary>
    [Endpoint("/config/v1/project/channels/{name}")]
    public class Channel : NamedUidEntity
    {
        public Channel()
        {

        }
        public Channel(string name)
        {
            Name = name;
        }
        /// <summary>
        /// Gets or sets the devices in the channel
        /// </summary>
        [YamlIgnore]
        [JsonPropertyName("devices")]
        [JsonPropertyOrder(100)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DeviceCollection? Devices { get; set; }

        /// <summary>
        /// Get the unique id key
        /// </summary>
        protected override string UniqueIdKey => Properties.NonUpdatable.ChannelUniqueId;

        /// <summary>
        /// Recursively cleans up the channel and all devices
        /// </summary>
        /// <param name="defaultValueProvider"></param>
        /// <param name="blnRemoveProjectId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task Cleanup(IKepwareDefaultValueProvider defaultValueProvider, bool blnRemoveProjectId = false, CancellationToken cancellationToken = default)
        {
            await base.Cleanup(defaultValueProvider, blnRemoveProjectId, cancellationToken).ConfigureAwait(false);

            if (Devices != null)
            {
                foreach (var device in Devices)
                {
                    await device.Cleanup(defaultValueProvider, blnRemoveProjectId, cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
