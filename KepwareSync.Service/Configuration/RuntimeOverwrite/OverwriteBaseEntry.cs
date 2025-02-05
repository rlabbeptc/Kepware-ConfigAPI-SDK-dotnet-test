using Kepware.Api.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Kepware.SyncService.Configuration.RuntimeOverwrite
{
    public abstract class OverwriteBaseEntry //: IYamlConvertible
    {
        /// <summary>
        /// Gets or sets the device name.
        /// </summary>
        [YamlMember]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of overwrite entries for the device.
        /// </summary>
        [YamlMember(Alias = "Overwrite")]
        public List<OverwriteProperty> Overwrite { get; set; } = new List<OverwriteProperty>();

        /// <inheritdoc/>
        public abstract void Read(IParser parser, Type expectedType, ObjectDeserializer nestedObjectDeserializer);


        /// <inheritdoc/>
        public virtual void Write(IEmitter emitter, ObjectSerializer nestedObjectSerializer)
        {
            // not needed as we only support reading
            throw new NotImplementedException();
        }

        public virtual bool Apply(BaseEntity entity)
        {
            var prevHash = entity.Hash;

            foreach (var overwrite in Overwrite)
            {
                if (entity.DynamicProperties.TryGetValue(overwrite.Key, out var property))
                {
                    switch (property.ValueKind)
                    {
                        default:
                            entity.SetDynamicProperty(overwrite.Key, overwrite.Value);
                            break;

                        case JsonValueKind.Number:
                            if (Int64.TryParse(overwrite.Value, out var longNumber))
                                entity.SetDynamicProperty(overwrite.Key, longNumber);
                            else if (Int32.TryParse(overwrite.Value, out var intNumber))
                                entity.SetDynamicProperty(overwrite.Key, intNumber);
                            else if (float.TryParse(overwrite.Value, out var floatNumber))
                                entity.SetDynamicProperty(overwrite.Key, floatNumber);
                            else if (float.TryParse(overwrite.Value, out var doublNumber))
                                entity.SetDynamicProperty(overwrite.Key, doublNumber);
                            else
                                throw new InvalidCastException($"Could not convert {overwrite.Value} to a number for {overwrite.Key} on {Name}");
                            break;

                        case JsonValueKind.True:
                        case JsonValueKind.False:
                            entity.SetDynamicProperty(overwrite.Key, overwrite.GetValueAsBool());

                            break;
                    }
                }
                else
                {
                    entity.SetDynamicProperty(overwrite.Key, overwrite.Value);
                }
            }

            return prevHash != entity.Hash;
        }
    }
}
