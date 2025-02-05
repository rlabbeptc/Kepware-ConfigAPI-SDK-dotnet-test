using Kepware.Api.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core.Events;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using Kepware.Api.Serializer;

namespace Kepware.SyncService.Configuration.RuntimeOverwrite
{
    /// <summary>
    /// Represents a tag overwrite entry that supports all CSV-based tag properties.
    /// </summary>
    public class OverwriteTagEntry : OverwriteBaseEntry
    {

        /// <summary>
        /// Maps CSV header keys (from <see cref="CsvTagSerializer.CsvHeaders"/>) to dynamic property keys (from <see cref="Properties.Tag"/>).
        /// </summary>
        private static readonly Dictionary<string, string> CsvHeaderToDynamicPropertyMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { CsvTagSerializer.CsvHeaders.Address,           Properties.Tag.Address },
            { CsvTagSerializer.CsvHeaders.DataType,          Properties.Tag.DataType },
            { CsvTagSerializer.CsvHeaders.ClientAccess,      Properties.Tag.ReadWriteAccess },
            { CsvTagSerializer.CsvHeaders.ScanRate,          Properties.Tag.ScanRateMilliseconds },
            { CsvTagSerializer.CsvHeaders.Scaling,           Properties.Tag.ScalingType },
            { CsvTagSerializer.CsvHeaders.RawLow,            Properties.Tag.ScalingRawLow },
            { CsvTagSerializer.CsvHeaders.RawHigh,           Properties.Tag.ScalingRawHigh },
            { CsvTagSerializer.CsvHeaders.ScaledLow,         Properties.Tag.ScalingScaledLow },
            { CsvTagSerializer.CsvHeaders.ScaledHigh,        Properties.Tag.ScalingScaledHigh },
            { CsvTagSerializer.CsvHeaders.ScaledDataType,    Properties.Tag.ScalingScaledDataType },
            { CsvTagSerializer.CsvHeaders.ClampLow,          Properties.Tag.ScalingClampLow },
            { CsvTagSerializer.CsvHeaders.ClampHigh,         Properties.Tag.ScalingClampHigh },
            { CsvTagSerializer.CsvHeaders.EngUnits,          Properties.Tag.ScalingUnits },
            { CsvTagSerializer.CsvHeaders.Description,       CsvTagSerializer.CsvHeaders.Description },
            { CsvTagSerializer.CsvHeaders.NegateValue,         Properties.Tag.ScalingNegateValue }
        };

        /// <inheritdoc/>
        public override void Read(IParser parser, Type expectedType, ObjectDeserializer nestedObjectDeserializer)
        {
            // The tag is defined as a mapping with a single key (the outer tag name)
            // whose value is another mapping of CSV header keys.
            parser.Consume<MappingStart>();
            Name = parser.Consume<Scalar>().Value;

            parser.Consume<MappingStart>();
            while (!parser.Accept<MappingEnd>(out _))
            {
                string key = parser.Consume<Scalar>().Value;
                string value = parser.Consume<Scalar>().Value;
                Overwrite.Add(new OverwriteProperty
                {
                    Key = key,
                    Value = value
                });
            }
            parser.Consume<MappingEnd>();
            parser.Consume<MappingEnd>();
        }

        public override bool Apply(BaseEntity entity)
        {
            if (entity is Tag tag)
            {
                var hash = tag.Hash;
                foreach (var overwrite in Overwrite)
                {
                    if (CsvHeaderToDynamicPropertyMapping.TryGetValue(overwrite.Key, out var dynamicKey))
                    {
                        switch (dynamicKey)
                        {
                            case Properties.Tag.Address:
                            case Properties.Tag.ScalingUnits:
                                tag.SetDynamicProperty(dynamicKey, overwrite.Value);
                                break;
                            case Properties.Tag.DataType:
                            case Properties.Tag.ScanRateMilliseconds:
                            case Properties.Tag.ScalingType:
                            case Properties.Tag.ScalingRawLow:
                            case Properties.Tag.ScalingRawHigh:
                            case Properties.Tag.ScalingScaledLow:
                            case Properties.Tag.ScalingScaledHigh:
                            case Properties.Tag.ScalingScaledDataType:
                                tag.SetDynamicProperty(dynamicKey, overwrite.GetValueAsInt());
                                break;

                            case Properties.Tag.ScalingClampLow:
                            case Properties.Tag.ScalingClampHigh:
                            case Properties.Tag.ScalingNegateValue:
                                tag.SetDynamicProperty(dynamicKey, overwrite.GetValueAsBool());
                                break;

                            default:
                                throw new InvalidOperationException($"Cannot apply {nameof(OverwriteTagEntry)} to entity of type {entity.GetType().Name} with key {overwrite.Key} mapped to {dynamicKey}.");
                        }

                    }
                }
                return hash != tag.Hash;
            }
            else
            {
                throw new InvalidOperationException($"Cannot apply {nameof(OverwriteTagEntry)} to entity of type {entity.GetType().Name}.");
            }
        }
    }
}
