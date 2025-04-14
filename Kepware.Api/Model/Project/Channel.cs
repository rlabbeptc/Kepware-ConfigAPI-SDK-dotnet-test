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
    /// Represents a channel in the project.
    /// </summary>
    [Endpoint("/config/v1/project/channels/{name}")]
    public class Channel : NamedUidEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Channel"/> class.
        /// </summary>
        public Channel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Channel"/> class with the specified name.
        /// </summary>
        /// <param name="name">The name of the channel.</param>
        public Channel(string name)
        {
            Name = name;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the devices in the channel.
        /// </summary>
        [YamlIgnore]
        [JsonPropertyName("devices")]
        [JsonPropertyOrder(100)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DeviceCollection? Devices { get; set; }

        /// <summary>
        /// Gets or sets the driver used by this channel.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public string? DeviceDriver
        {
            get => GetDynamicProperty<string>(Properties.Channel.DeviceDriver);
            set => SetDynamicProperty(Properties.Channel.DeviceDriver, value);
        }

        /// <summary>
        /// Gets or sets the network adapter used for Ethernet-based communication.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public string? EthernetNetworkAdapter
        {
            get => GetDynamicProperty<string>(Properties.Channel.EthernetNetworkAdapter);
            set => SetDynamicProperty(Properties.Channel.EthernetNetworkAdapter, value);
        }

        /// <summary>
        /// Gets or sets how non-normalized IEEE-754 floating point values are handled.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public string? NonNormalizedFloatHandling
        {
            get => GetDynamicProperty<string>(Properties.Channel.NonNormalizedFloatHandling);
            set => SetDynamicProperty(Properties.Channel.NonNormalizedFloatHandling, value);
        }

        /// <summary>
        /// Gets or sets the write optimization method used for the channel.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public string? WriteOptimizationsMethod
        {
            get => GetDynamicProperty<string>(Properties.Channel.WriteOptimizationsMethod);
            set => SetDynamicProperty(Properties.Channel.WriteOptimizationsMethod, value);
        }

        /// <summary>
        /// Gets or sets the write optimization duty cycle.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public int? WriteOptimizationsDutyCycle
        {
            get => GetDynamicProperty<int>(Properties.Channel.WriteOptimizationsDutyCycle);
            set => SetDynamicProperty(Properties.Channel.WriteOptimizationsDutyCycle, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether communication diagnostics is enabled for the channel.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public bool? DiagnosticsCapture
        {
            get => GetDynamicProperty<bool>(Properties.Channel.DiagnosticsCapture);
            set => SetDynamicProperty(Properties.Channel.DiagnosticsCapture, value);
        }

        #endregion

        /// <summary>
        /// Gets the unique ID key for the channel.
        /// </summary>
        protected override string UniqueIdKey => Properties.NonUpdatable.ChannelUniqueId;

        /// <summary>
        /// Recursively cleans up the channel and all devices.
        /// </summary>
        /// <param name="defaultValueProvider">The default value provider.</param>
        /// <param name="blnRemoveProjectId">Whether to remove the project ID.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous cleanup operation.</returns>
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
