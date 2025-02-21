using Kepware.Api.Serializer;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using YamlDotNet.Core.Tokens;
using YamlDotNet.Serialization;

namespace Kepware.Api.Model
{
    /// <summary>
    /// Represents a project in the Kepware configuration
    /// </summary>
    [Endpoint("/config/v1/project")]
    public class Project : BaseEntity
    {
        /// <summary>
        /// If this is true the project was loaded by the JsonProjectLoad service (added to Kepware Server v6.17 / Kepware Edge v1.10)
        /// </summary>
        public bool IsLoadedByProjectLoadService { get; internal set; } = false;

        /// <summary>
        /// Gets or sets the channels in the project
        /// </summary>
        [YamlIgnore]
        [JsonPropertyName("channels")]
        [JsonPropertyOrder(100)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ChannelCollection? Channels { get; set; }

        /// <summary>
        /// Recursively cleans up the project and all its children
        /// </summary>
        /// <param name="defaultValueProvider"></param>
        /// <param name="blnRemoveProjectId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task Cleanup(IKepwareDefaultValueProvider defaultValueProvider, bool blnRemoveProjectId = false, CancellationToken cancellationToken = default)
        {
            await base.Cleanup(defaultValueProvider, blnRemoveProjectId, cancellationToken).ConfigureAwait(false);


            if (Channels != null)
            {
                foreach (var channel in Channels)
                {
                    await channel.Cleanup(defaultValueProvider, blnRemoveProjectId, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public async Task<Project> CloneAsync(CancellationToken cancellationToken = default)
        {
            using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, this, KepJsonContext.Default.Project, cancellationToken).ConfigureAwait(false);
            stream.Position = 0;

            return await JsonSerializer.DeserializeAsync(stream, KepJsonContext.Default.Project, cancellationToken).ConfigureAwait(false) ??
                throw new InvalidOperationException("CloneAsync failed");
        }

    }

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

    /// <summary>
    /// Represents a device in a channel
    /// </summary>
    [Endpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}")]
    public class Device : NamedUidEntity
    {
        public Device()
        {

        }

        public Device(string name, Channel channel)
        {
            Name = name;
            Owner = channel;
        }

        public Device(string name, string channelName)
            : this(name, new Channel(channelName))
        {
        }

        /// <summary>
        /// Gets or sets the tags in the device
        /// </summary>
        [YamlIgnore]
        [JsonPropertyName("tags")]
        [JsonPropertyOrder(100)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DeviceTagCollection? Tags { get; set; }

        /// <summary>
        /// Gets or sets the tag groups in the device
        /// </summary>
        [YamlIgnore]
        [JsonPropertyName("tag_groups")]
        [JsonPropertyOrder(200)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DeviceTagGroupCollection? TagGroups { get; set; }

        /// <summary>
        /// Gets the unique id key
        /// </summary>
        protected override string UniqueIdKey => Properties.NonUpdatable.DeviceUniqueId;


        /// <summary>
        /// Recursively cleans up the device and all its children
        /// </summary>
        /// <param name="defaultValueProvider"></param>
        /// <param name="blnRemoveProjectId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task Cleanup(IKepwareDefaultValueProvider defaultValueProvider, bool blnRemoveProjectId = false, CancellationToken cancellationToken = default)
        {
            await base.Cleanup(defaultValueProvider, blnRemoveProjectId, cancellationToken).ConfigureAwait(false);

            if (Tags != null)
            {
                foreach (var tag in Tags)
                {
                    await tag.Cleanup(defaultValueProvider, blnRemoveProjectId, cancellationToken).ConfigureAwait(false);
                }
            }

            if (TagGroups != null)
            {
                foreach (var tagGroup in TagGroups)
                {
                    await tagGroup.Cleanup(defaultValueProvider, blnRemoveProjectId, cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }

    /// <summary>
    /// Represents a tag group in a device
    /// </summary>
    [RecursiveEndpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}", "/tag_groups/{groupName}", typeof(DeviceTagGroup))]
    public class DeviceTagGroup : NamedEntity
    {
        public DeviceTagGroup()
        {

        }

        public DeviceTagGroup(string name, Device owner)
        {
            Owner = owner;
            Name = name;
        }

        public DeviceTagGroup(string name, DeviceTagGroup owner)
        {
            Owner = owner;
            Name = name;
        }

        /// <summary>
        /// Gets or sets the tags in the tag group
        /// </summary>
        [YamlIgnore]
        [JsonPropertyName("tags")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DeviceTagGroupTagCollection? Tags { get; set; }

        /// <summary>
        /// Recursively cleans up the tag group and all its children
        /// </summary>
        [YamlIgnore]
        [JsonPropertyName("tag_groups")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DeviceTagGroupCollection? TagGroups { get; set; }

        /// <summary>
        /// Get a flag indicating if the tag group is autogenerated
        /// </summary>
        [YamlIgnore]
        [JsonIgnore]
        public bool IsAutogenerated => GetDynamicProperty<bool>(Properties.NonSerialized.TagGroupAutogenerated);

        /// <summary>
        /// Recursively cleans up the tag group and all its children
        /// </summary>
        /// <param name="defaultValueProvider"></param>
        /// <param name="blnRemoveProjectId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task Cleanup(IKepwareDefaultValueProvider defaultValueProvider, bool blnRemoveProjectId = false, CancellationToken cancellationToken = default)
        {
            await base.Cleanup(defaultValueProvider, blnRemoveProjectId, cancellationToken).ConfigureAwait(false);

            if (Tags != null)
            {
                foreach (var tag in Tags)
                {
                    await tag.Cleanup(defaultValueProvider, blnRemoveProjectId, cancellationToken).ConfigureAwait(false);
                }
            }

            if (TagGroups != null)
            {
                foreach (var tagGroup in TagGroups)
                {
                    await tagGroup.Cleanup(defaultValueProvider, blnRemoveProjectId, cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }

    /// <summary>
    /// Represents a tag in a device or tag group
    /// </summary>
    [RecursiveEndpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}", "/tag_groups/{groupName}", typeof(DeviceTagGroup), suffix: "/tags/{tagName}")]
    public class Tag : NamedEntity
    {
        /// <summary>
        /// A flag indicating if the tag is autogenerated
        /// </summary>
        [YamlIgnore]
        [JsonIgnore]
        public bool IsAutogenerated => GetDynamicProperty<bool>(Properties.NonSerialized.TagAutogenerated) == true;

        /// <summary>
        /// Gets or sets the tag address
        /// </summary>
        [YamlIgnore]
        [JsonIgnore]
        public string TagAddress
        {
            get => GetDynamicProperty<string>(Properties.Tag.Address) ?? string.Empty;
            set => SetDynamicProperty(Properties.Tag.Address, value);
        }

        /// <summary>
        /// If the tag has no scaling the scaling properties are not serialized
        /// </summary>
        /// <returns></returns>
        protected override ISet<string>? ConditionalNonSerialized()
        {
            if (GetDynamicProperty<int>(Properties.Tag.ScalingType) == 0)
            {
                return Properties.Tag.IgnoreWhenScalingDisalbedHashSet;
            }
            return null;
        }
    }

    /// <summary>
    /// Represents the collection of channels in a project
    /// </summary>
    [Endpoint("/config/v1/project/channels")]
    public class ChannelCollection : EntityCollection<Channel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelCollection"/> class.
        /// </summary>
        public ChannelCollection() { }

    }

    /// <summary>
    /// Represents the collection of devices in a channel
    /// </summary>
    [Endpoint("/config/v1/project/channels/{channelName}/devices")]
    public class DeviceCollection : EntityCollection<Device>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceCollection"/> class.
        /// </summary>
        public DeviceCollection() { }
    }

    /// <summary>
    /// Represents the collection of tags in a device
    /// </summary>
    [Endpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}/tags")]
    public class DeviceTagCollection : EntityCollection<Tag>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceTagCollection"/> class.
        /// </summary>
        public DeviceTagCollection() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceTagCollection"/> class.
        /// </summary>
        /// <param name="collection"></param>
        public DeviceTagCollection(IEnumerable<Tag> collection)
            : base(collection)
        {

        }
    }

    /// <summary>
    /// Represents the collection of consumer exchange groups in a device
    /// </summary>
    [Endpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}/consumer_exchange_groups")]
    public class ConsumerExchangeGroupCollection : EntityCollection<DefaultEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsumerExchangeGroupCollection"/> class.
        /// </summary>
        public ConsumerExchangeGroupCollection() { }
    }

    /// <summary>
    /// Represents the collection of consumer exchanges in a consumer exchange group
    /// </summary>
    [Endpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}/consumer_exchange_groups/{groupName}/consumer_exchanges")]
    public class ConsumerExchangeCollection : EntityCollection<DefaultEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsumerExchangeCollection"/> class.
        /// </summary>
        public ConsumerExchangeCollection() { }
    }

    /// <summary>
    /// Represents the collection of producer exchange groups in a device
    /// </summary>
    [Endpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}/producer_exchange_groups")]
    public class ProducerExchangeGroupCollection : EntityCollection<DefaultEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProducerExchangeGroupCollection"/> class.
        /// </summary>
        public ProducerExchangeGroupCollection() { }
    }

    /// <summary>
    /// Represents the collection of producer exchanges in a producer exchange group
    /// </summary>
    [Endpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}/producer_exchange_groups/{groupName}/producer_exchanges")]
    public class ProducerExchangeCollection : EntityCollection<DefaultEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProducerExchangeCollection"/> class.
        /// </summary>
        public ProducerExchangeCollection() { }
    }

    /// <summary>
    /// Represents the collection of omni mapping groups in a device
    /// </summary>
    [Endpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}/omni_mapping_groups")]
    public class OmniMappingGroupCollection : EntityCollection<DefaultEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OmniMappingGroupCollection"/> class.
        /// </summary>
        public OmniMappingGroupCollection() { }
    }

    /// <summary>
    /// Represents the collection of omni mappings in an omni mapping group
    /// </summary>
    [Endpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}/omni_mapping_groups/{groupName}/omni_mappings")]
    public class OmniMappingCollection : EntityCollection<DefaultEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OmniMappingCollection"/> class.
        /// </summary>
        public OmniMappingCollection() { }
    }

    /// <summary>
    /// Represents the collection of omni gas alarms in an omni mapping
    /// </summary>
    [Endpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}/omni_mapping_groups/{groupName}/omni_mappings/{mappingName}/omni_gas_alarms")]
    public class OmniAlarmCollection : EntityCollection<DefaultEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OmniAlarmCollection"/> class.
        /// </summary>
        public OmniAlarmCollection() { }
    }

    /// <summary>
    /// Represents the collection of tag groups in a device or tag group
    /// </summary>
    [RecursiveEndpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}", "/tag_groups/{groupName}", typeof(DeviceTagGroup), suffix: "/tag_groups")]
    public class DeviceTagGroupCollection : EntityCollection<DeviceTagGroup>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceTagGroupCollection"/> class.
        /// </summary>
        public DeviceTagGroupCollection() { }
    }

    /// <summary>
    /// Represents the collection of tag in a tag group
    /// </summary>
    [RecursiveEndpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}", "/tag_groups/{groupName}", typeof(DeviceTagGroup), suffix: "/tags")]
    public class DeviceTagGroupTagCollection : EntityCollection<Tag>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceTagGroupTagCollection"/> class.
        /// </summary>
        public DeviceTagGroupTagCollection() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceTagGroupTagCollection"/> class.
        /// </summary>
        public DeviceTagGroupTagCollection(IEnumerable<Tag> collection)
            : base(collection)
        {

        }

    }

    /// <summary>
    /// Represents the collection of phonebooks in a channel
    /// </summary>
    [Endpoint("/config/v1/project/channels/{channelName}/phonebooks")]
    public class PhonebookCollection : EntityCollection<DefaultEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PhonebookCollection"/> class.
        /// </summary>
        public PhonebookCollection() { }
    }

    /// <summary>
    /// Represents the collection of phone entries in a phonebook
    /// </summary>
    [Endpoint("/config/v1/project/channels/{channelName}/phonebooks/{phonebookName}/phonelist")]
    public class PhoneEntryCollection : EntityCollection<DefaultEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneEntryCollection"/> class.
        /// </summary>
        public PhoneEntryCollection() { }
    }

    /// <summary>
    /// Represents the collection of deviceprofiles in a device
    /// </summary>
    [Endpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}/device_profiles")]
    public class DeviceProfileCollection : EntityCollection<DefaultEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceProfileCollection"/> class.
        /// </summary>
        public DeviceProfileCollection() { }
    }
}
