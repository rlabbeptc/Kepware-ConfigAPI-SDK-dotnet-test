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

        public object? ReadYaml(IParser parser, Type type, ObjectDeserializer nestedObjectDeserializer)
        {
            if (!typeof(BaseEntity).IsAssignableFrom(type))
            {
                throw new InvalidOperationException($"Cannot deserialize type {type}");
            }

            var entity = (BaseEntity)Activator.CreateInstance(type)!;

            // Erwartet den Start eines Mappings
            if (!(parser.Current is MappingStart))
            {
                throw new YamlException("Expected MappingStart");
            }

            while (parser.MoveNext())
            {
                if (parser.Current is DocumentEnd)
                {
                    break;
                }

                if (parser.Current is MappingEnd)
                {
                    continue;
                }

                if (!(parser.Current is Scalar keyScalar))
                {
                    throw new YamlException("Expected Scalar for key");
                }

                string key = keyScalar.Value ?? throw new YamlException("Key cannot be null");

                if (!parser.MoveNext())
                {
                    throw new YamlException("Unexpected end of YAML while expecting a value");
                }

                object? value = DeserializeValue(parser, nestedObjectDeserializer);

                if (key == "common.ALLTYPES_DESCRIPTION")
                {
                    entity.Description = value?.ToString() ?? string.Empty;
                }
                else
                {
                    if (!m_nonpersistetDynamicProps.Contains(key))
                    {
                        entity.DynamicProperties[key] = KepJsonContext.WrapInJsonElement(value) ;
                    }
                }
            }

            return entity;
        }

        private object? DeserializeValue(IParser parser, ObjectDeserializer nestedObjectDeserializer)
        {
            switch (parser.Current)
            {
                case Scalar scalar:
                    string? scalarValue = scalar.Value;
                    if (int.TryParse(scalarValue, out var intValue))
                        return intValue;
                    if (long.TryParse(scalarValue, out var longValue))
                        return longValue;
                    if (float.TryParse(scalarValue, out var floatValue))
                        return floatValue;
                    if (double.TryParse(scalarValue, out var doubleValue))
                        return doubleValue;
                    if (bool.TryParse(scalarValue, out var boolValue))
                        return boolValue;
                    return scalarValue;

                case SequenceStart _:
                    var list = new List<object?>();
                    parser.MoveNext(); // Move past SequenceStart
                    while (!(parser.Current is SequenceEnd))
                    {
                        list.Add(DeserializeValue(parser, nestedObjectDeserializer));
                        parser.MoveNext();
                    }
                    return list;

                case MappingStart _:
                    var dict = new Dictionary<string, object?>();
                    parser.MoveNext(); // Move past MappingStart
                    while (!(parser.Current is MappingEnd))
                    {
                        if (!(parser.Current is Scalar mapKey))
                            throw new YamlException("Expected Scalar for dictionary key");
                        string mapKeyValue = mapKey.Value ?? throw new YamlException("Key cannot be null");

                        if (!parser.MoveNext())
                            throw new YamlException("Unexpected end of YAML while expecting a value");

                        object? mapValue = DeserializeValue(parser, nestedObjectDeserializer);
                        dict[mapKeyValue] = mapValue;

                        parser.MoveNext(); // Move to next key or MappingEnd
                    }
                    return dict;

                default:
                    throw new YamlException($"Unexpected YAML node type: {parser.Current?.GetType().Name ?? "unknown"}");
            }
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
