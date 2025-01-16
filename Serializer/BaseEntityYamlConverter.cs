using KepwareSync.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NodeTypeResolvers;

namespace KepwareSync.Serializer
{
    public class BaseEntityYamlTypeConverter : IYamlTypeConverter
    {
        private readonly IReadOnlySet<string> m_nonpersistetDynamicProps;
        public BaseEntityYamlTypeConverter(IReadOnlySet<string>? nonpersistetDynamicProps = null)
        {
            m_nonpersistetDynamicProps = nonpersistetDynamicProps ?? new HashSet<string>();
        }

        public bool Accepts(Type type)
        {
            return typeof(BaseEntity).IsAssignableFrom(type);
        }

        public object? ReadYaml(IParser parser, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type, ObjectDeserializer nestedObjectDeserializer)
        {
            if (!typeof(BaseEntity).IsAssignableFrom(type))
            {
                throw new InvalidOperationException($"Cannot deserialize type {type}");
            }

            var entity = (BaseEntity)Activator.CreateInstance(type)!;


            var yamlDictionary = nestedObjectDeserializer(typeof(Dictionary<string, object>)) as Dictionary<string, object>;

            if (yamlDictionary == null)
            {
                throw new YamlException("Expected a mapping to deserialize into Dictionary<string, object>");
            }

            foreach (var kvp in yamlDictionary)
            {
                //string based  to scalar types (int, float, bool, etc.)
                if (kvp.Value is string stringValue)
                {
                    if (int.TryParse(stringValue, out var intValue))
                    {
                        entity.DynamicProperties[kvp.Key] = intValue;
                    }
                    else if (float.TryParse(stringValue, out var floatValue))
                    {
                        entity.DynamicProperties[kvp.Key] = floatValue;
                    }
                    else if (double.TryParse(stringValue, out var doubleValue))
                    {
                        entity.DynamicProperties[kvp.Key] = doubleValue;
                    }
                    else if (bool.TryParse(stringValue, out var boolValue))
                    {
                        entity.DynamicProperties[kvp.Key] = boolValue;
                    }
                    else
                    {
                        entity.DynamicProperties[kvp.Key] = stringValue;
                    }
                }
                else
                {
                    entity.DynamicProperties[kvp.Key] = kvp.Value;
                }
            }

            if (yamlDictionary.TryGetValue(Properties.Description, out var description))
                entity.Description = description?.ToString() ?? string.Empty;

            return entity;
        }


        public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer nestedObjectSerializer)
        {
            if (value is BaseEntity entity)
            {
                emitter.Emit(new MappingStart());

                // Serialize known properties
                if (!string.IsNullOrEmpty(entity.Description))
                {
                    emitter.Emit(new Scalar("common.ALLTYPES_DESCRIPTION"));
                    emitter.Emit(new Scalar(entity.Description));
                }

                // Serialize DynamicProperties directly at the top level
                foreach (var property in entity.DynamicProperties)
                {
                    if (m_nonpersistetDynamicProps.Contains(property.Key))
                    {
                        continue;
                    }
                    emitter.Emit(new Scalar(property.Key));
                    SerializeJsonValue(emitter, property.Value, nestedObjectSerializer);
                }
                emitter.Emit(new MappingEnd());
            }
        }

        private void SerializeJsonValue(IEmitter emitter, object? value, ObjectSerializer nestedObjectSerializer)
        {
            switch (value)
            {
                case JsonNode jsonNode:
                    if (jsonNode is JsonArray jsonArray)
                    {
                        emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Block));
                        foreach (var item in jsonArray)
                        {
                            SerializeJsonValue(emitter, item, nestedObjectSerializer);
                        }
                        emitter.Emit(new SequenceEnd());
                    }
                    else if (jsonNode is JsonObject jsonObject)
                    {
                        nestedObjectSerializer(jsonObject);
                    }
                    else
                    {
                        emitter.Emit(new Scalar(jsonNode.ToString() ?? string.Empty)); // Scalar value
                    }
                    break;

                case JsonElement element:
                    switch (element.ValueKind)
                    {
                        case JsonValueKind.Array:
                            emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Block));
                            foreach (var item in element.EnumerateArray())
                            {
                                SerializeJsonValue(emitter, item, nestedObjectSerializer);
                            }
                            emitter.Emit(new SequenceEnd());
                            break;

                        case JsonValueKind.Object:
                            nestedObjectSerializer(element);
                            break;

                        case JsonValueKind.String:
                            emitter.Emit(new Scalar(element.GetString() ?? string.Empty));
                            break;

                        case JsonValueKind.Number:
                            emitter.Emit(new Scalar(element.GetRawText())); // Number as string
                            break;

                        case JsonValueKind.True:
                            emitter.Emit(new Scalar("true"));
                            break;

                        case JsonValueKind.False:
                            emitter.Emit(new Scalar("false"));
                            break;

                        case JsonValueKind.Null:
                            emitter.Emit(new Scalar(string.Empty)); // Null as empty value
                            break;

                        default:
                            emitter.Emit(new Scalar(element.GetRawText()));
                            break;
                    }
                    break;

                default:
                    emitter.Emit(new Scalar(value?.ToString() ?? string.Empty)); // Other types
                    break;
            }
        }
    }
}
