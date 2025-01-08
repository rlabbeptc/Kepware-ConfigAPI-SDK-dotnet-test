using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace KepwareSync.Model
{
    [Endpoint("/config/v1/project/channels")]
    public class Channel : BaseEntity
    {
        [JsonIgnore]
        [YamlIgnore]
        public List<Device> Devices { get; set; } = new();

        [JsonIgnore]
        [YamlIgnore]
        public List<Phonebook> Phonebooks { get; set; } = new();
    }

    [Endpoint("/config/v1/project/channels/{channelName}/devices")]
    public class Device : BaseEntity
    {
        [JsonIgnore]
        [YamlIgnore]
        public List<ConsumerExchangeGroup> ConsumerExchangeGroups { get; set; } = new();

        [JsonIgnore]
        [YamlIgnore]
        public List<DeviceProfile> DeviceProfiles { get; set; } = new();

        [JsonIgnore]
        [YamlIgnore]
        public List<TagGroup> TagGroups { get; set; } = new();

        [JsonIgnore]
        [YamlIgnore]
        public List<OmniMappingGroup> OmniMappingGroups { get; set; } = new();

        [JsonIgnore]
        [YamlIgnore]
        public List<ProducerExchangeGroup> ProducerExchangeGroups { get; set; } = new();
    }

    [Endpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}/consumer_exchange_groups")]
    public class ConsumerExchangeGroup : BaseEntity
    {
        [JsonIgnore]
        [YamlIgnore]
        public List<ConsumerExchange> ConsumerExchanges { get; set; } = new();
    }

    [Endpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}/consumer_exchange_groups/{groupName}/consumer_exchanges")]
    public class ConsumerExchange : BaseEntity
    {
        [JsonIgnore]
        [YamlIgnore]
        public List<Range> Ranges { get; set; } = new();
    }

    [Endpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}/producer_exchange_groups")]
    public class ProducerExchangeGroup : BaseEntity
    {
        [JsonIgnore]
        [YamlIgnore]
        public List<ProducerExchange> ProducerExchanges { get; set; } = new();
    }

    [Endpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}/producer_exchange_groups/{groupName}/producer_exchanges")]
    public class ProducerExchange : BaseEntity
    {
        [JsonIgnore]
        [YamlIgnore]
        public List<Range> Ranges { get; set; } = new();
    }

    [Endpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}/omni_mapping_groups")]
    public class OmniMappingGroup : BaseEntity
    {
        [JsonIgnore]
        [YamlIgnore]
        public List<OmniMapping> OmniMappings { get; set; } = new();
    }

    [Endpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}/omni_mapping_groups/{groupName}/omni_mappings")]
    public class OmniMapping : BaseEntity
    {
        [JsonIgnore]
        [YamlIgnore]
        public List<OmniAlarm> OmniAlarms { get; set; } = new();
    }

    [Endpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}/omni_mapping_groups/{groupName}/omni_mappings/{mappingName}/omni_gas_alarms")]
    public class OmniAlarm : BaseEntity
    {
        // Specific properties for Omni Alarms
    }

    [Endpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}/tag_groups")]
    public class TagGroup : BaseEntity
    {
        [JsonIgnore]
        [YamlIgnore]
        public List<Tag> Tags { get; set; } = new();
    }

    [Endpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}/tag_groups/{groupName}/tags")]
    public class Tag : BaseEntity { }

    [Endpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}/tag_groups/{groupName}/tags/{tagName}/ranges")]
    public class Range : BaseEntity { }

    [Endpoint("/config/v1/project/channels/{channelName}/phonebooks")]
    public class Phonebook : BaseEntity
    {
        [JsonIgnore]
        [YamlIgnore]
        public List<PhoneEntry> PhoneEntries { get; set; } = new();
    }

    [Endpoint("/config/v1/project/channels/{channelName}/phonebooks/{phonebookName}/phonelist")]
    public class PhoneEntry : BaseEntity { }

    [Endpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}/device_profiles")]
    public class DeviceProfile : BaseEntity { }
}
