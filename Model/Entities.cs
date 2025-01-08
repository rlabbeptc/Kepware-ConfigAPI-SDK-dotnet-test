using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using YamlDotNet.Core.Tokens;
using YamlDotNet.Serialization;

namespace KepwareSync.Model
{
    [Endpoint("/config/v1/project/channels/{name}")]
    public class Channel : BaseEntity
    {
    }

    [Endpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}")]
    public class Device(BaseEntity owner) : DefaultEntity(owner) { }

    [Endpoint("/config/v1/project/channels")]
    public class ChannelCollection : EntityCollection<Channel>
    {
        public ChannelCollection(BaseEntity owner) : base(owner) { }
    }

    [Endpoint("/config/v1/project/channels/{channelName}/devices")]
    public class DeviceCollection : EntityCollection<Device>
    {
        public DeviceCollection(BaseEntity owner) : base(owner) { }
    }

    [Endpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}/tags")]
    public class DeviceTagCollection : EntityCollection<DefaultEntity>
    {
        public DeviceTagCollection(BaseEntity owner) : base(owner) { }
    }

    [Endpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}/consumer_exchange_groups")]
    public class ConsumerExchangeGroupCollection : EntityCollection<DefaultEntity>
    {
        public ConsumerExchangeGroupCollection(BaseEntity owner) : base(owner) { }
    }

    [Endpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}/consumer_exchange_groups/{groupName}/consumer_exchanges")]
    public class ConsumerExchangeCollection : EntityCollection<DefaultEntity>
    {
        public ConsumerExchangeCollection(BaseEntity owner) : base(owner) { }
    }

    [Endpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}/producer_exchange_groups")]
    public class ProducerExchangeGroupCollection : EntityCollection<DefaultEntity>
    {
        public ProducerExchangeGroupCollection(BaseEntity owner) : base(owner) { }
    }

    [Endpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}/producer_exchange_groups/{groupName}/producer_exchanges")]
    public class ProducerExchangeCollection : EntityCollection<DefaultEntity>
    {
        public ProducerExchangeCollection(BaseEntity owner) : base(owner) { }
    }

    [Endpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}/omni_mapping_groups")]
    public class OmniMappingGroupCollection : EntityCollection<DefaultEntity>
    {
        public OmniMappingGroupCollection(BaseEntity owner) : base(owner) { }
    }

    [Endpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}/omni_mapping_groups/{groupName}/omni_mappings")]
    public class OmniMappingCollection : EntityCollection<DefaultEntity>
    {
        public OmniMappingCollection(BaseEntity owner) : base(owner) { }
    }

    [Endpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}/omni_mapping_groups/{groupName}/omni_mappings/{mappingName}/omni_gas_alarms")]
    public class OmniAlarmCollection : EntityCollection<DefaultEntity>
    {
        public OmniAlarmCollection(BaseEntity owner) : base(owner) { }
    }

    [Endpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}/tag_groups")]
    public class TagGroupCollection : EntityCollection<DefaultEntity>
    {
        public TagGroupCollection(BaseEntity owner) : base(owner) { }
    }

    [Endpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}/tag_groups/{groupName}/tags")]
    public class TagCollection : EntityCollection<DefaultEntity>
    {
        public TagCollection(BaseEntity owner) : base(owner) { }
    }

    [Endpoint("/config/v1/project/channels/{channelName}/phonebooks")]
    public class PhonebookCollection : EntityCollection<DefaultEntity>
    {
        public PhonebookCollection(BaseEntity owner) : base(owner) { }
    }

    [Endpoint("/config/v1/project/channels/{channelName}/phonebooks/{phonebookName}/phonelist")]
    public class PhoneEntryCollection : EntityCollection<DefaultEntity>
    {
        public PhoneEntryCollection(BaseEntity owner) : base(owner) { }
    }

    [Endpoint("/config/v1/project/channels/{channelName}/devices/{deviceName}/device_profiles")]
    public class DeviceProfileCollection : EntityCollection<DefaultEntity>
    {
        public DeviceProfileCollection(BaseEntity owner) : base(owner) { }
    }
}
