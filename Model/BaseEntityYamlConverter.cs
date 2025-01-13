using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NodeTypeResolvers;

namespace KepwareSync.Model
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
            throw new NotImplementedException("Deserialization is not implemented.");
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer nestedObjectSerializer)
        {
            if (value is BaseEntity entity)
            {
                emitter.Emit(new MappingStart());

                // Serialisiere bekannte Eigenschaften
                if (!string.IsNullOrEmpty(entity.Description))
                {
                    emitter.Emit(new Scalar("common.ALLTYPES_DESCRIPTION"));
                    emitter.Emit(new Scalar(entity.Description));
                }

                // Serialisiere DynamicProperties direkt auf der obersten Ebene
                foreach (var property in entity.DynamicProperties)
                {
                    if(m_nonpersistetDynamicProps.Contains(property.Key))
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
                        emitter.Emit(new Scalar(jsonNode.ToString() ?? string.Empty)); // Skalarwert
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
                            emitter.Emit(new Scalar(element.GetRawText())); // Zahl als String
                            break;

                        case JsonValueKind.True:
                            emitter.Emit(new Scalar("true"));
                            break;

                        case JsonValueKind.False:
                            emitter.Emit(new Scalar("false"));
                            break;

                        case JsonValueKind.Null:
                            emitter.Emit(new Scalar(string.Empty)); // Null als leerer Wert
                            break;

                        default:
                            emitter.Emit(new Scalar(element.GetRawText()));
                            break;
                    }
                    break;

                default:
                    emitter.Emit(new Scalar(value?.ToString() ?? string.Empty)); // Andere Typen
                    break;
            }
        }

    }
}
