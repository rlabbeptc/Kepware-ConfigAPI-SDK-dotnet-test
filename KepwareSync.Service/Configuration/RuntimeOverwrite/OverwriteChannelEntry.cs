using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core.Events;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using Kepware.Api.Model;

namespace Kepware.SyncService.Configuration.RuntimeOverwrite
{
    /// <summary>
    /// Represents a channel configuration with optional overwrite entries and a list of devices.
    /// </summary>
    public class OverwriteChannelEntry : OverwriteBaseEntry
    {
        /// <summary>
        /// Gets or sets the list of devices under the channel.
        /// </summary>
        [YamlMember(Alias = "Devices")]
        public List<OverwriteDeviceEntry> Devices { get; set; } = new List<OverwriteDeviceEntry>();

        /// <inheritdoc/>
        public override void Read(IParser parser, Type expectedType, ObjectDeserializer nestedObjectDeserializer)
        {
            parser.Consume<MappingStart>();
            Name = parser.Consume<Scalar>().Value;

            while (!parser.Accept<MappingEnd>(out _))
            {
                string propertyName = parser.Consume<Scalar>().Value;
                if (propertyName.Equals(nameof(Overwrite), StringComparison.Ordinal))
                {
                    parser.Consume<SequenceStart>();
                    while (!parser.Accept<SequenceEnd>(out _))
                    {
                        var entry = (OverwriteProperty)nestedObjectDeserializer(typeof(OverwriteProperty))!;
                        Overwrite.Add(entry);
                    }
                    parser.Consume<SequenceEnd>();
                }
                else if (propertyName.Equals(nameof(Devices), StringComparison.Ordinal))
                {
                    parser.Consume<SequenceStart>();
                    while (!parser.Accept<SequenceEnd>(out _))
                    {
                        var device = (OverwriteDeviceEntry)nestedObjectDeserializer(typeof(OverwriteDeviceEntry))!;
                        Devices.Add(device);
                    }
                    parser.Consume<SequenceEnd>();
                }
                else
                {
                    nestedObjectDeserializer(typeof(object));
                }
            }
            parser.Consume<MappingEnd>();
        }


        public override bool Apply(BaseEntity entity)
        {

            if (entity is Channel channel)
            {
                bool blnRet = base.Apply(entity);
                var deviceMap = channel.Devices?.ToDictionary(d => d.Name, StringComparer.InvariantCultureIgnoreCase) ?? [];
                foreach (var deviceOverwrite in this.Devices)
                {
                    if (deviceMap.TryGetValue(deviceOverwrite.Name, out var device))
                    {
                        blnRet |= deviceOverwrite.Apply(device);
                    }
                }

                return blnRet;
            }
            else
            {
                throw new InvalidOperationException($"Cannot apply {nameof(OverwriteChannelEntry)} to entity of type {entity.GetType().Name}.");
            }
        }
    }

}
