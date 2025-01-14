using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;
using YamlDotNet.Serialization;

namespace KepwareSync.Model
{
    public interface IHaveOwner
    {
        [JsonIgnore]
        [YamlIgnore]
        public BaseEntity? Owner { get; internal set; }
    }


    [DebuggerDisplay("{Name} - {Description}")]
    public abstract class BaseEntity
    {
        [JsonPropertyName(Properties.ProjectId)]
        [YamlIgnore]
        public long? ProjectId { get; set; } = null;

        [JsonPropertyName(Properties.Name)]
        [YamlIgnore]
        //Yaml File-Name
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName(Properties.Description)]
        [YamlMember(Alias = Properties.Description)]
        public string? Description { get; set; } = string.Empty;

        [JsonExtensionData]
        //Yaml-Properties
        public Dictionary<string, object?> DynamicProperties { get; set; } = [];

        public T? GetDynamicProperty<T>(string key)
        {
            if (DynamicProperties.TryGetValue(key, out var value))
            {
                if (value is JsonElement jsonElement)
                {
                    if (typeof(T) == typeof(bool))
                        return (T?)(object)jsonElement.GetBoolean();
                    if (typeof(T) == typeof(byte))
                        return (T?)(object)jsonElement.GetByte();
                    if (typeof(T) == typeof(sbyte))
                        return (T?)(object)jsonElement.GetSByte();
                    if (typeof(T) == typeof(short))
                        return (T?)(object)jsonElement.GetInt16();
                    if (typeof(T) == typeof(ushort))
                        return (T?)(object)jsonElement.GetUInt16();
                    if (typeof(T) == typeof(int))
                        return (T?)(object)jsonElement.GetInt32();
                    if (typeof(T) == typeof(uint))
                        return (T?)(object)jsonElement.GetUInt32();
                    if (typeof(T) == typeof(long))
                        return (T?)(object)jsonElement.GetInt64();
                    if (typeof(T) == typeof(ulong))
                        return (T?)(object)jsonElement.GetUInt64();
                    if (typeof(T) == typeof(float))
                        return (T?)(object)jsonElement.GetSingle();
                    if (typeof(T) == typeof(double))
                        return (T?)(object)jsonElement.GetDouble();
                    if (typeof(T) == typeof(string))
                        return (T?)(object?)jsonElement.GetString();

                    throw new NotSupportedException($"Type '{typeof(T)}' is not supported.");
                }

                // Directly attempt to cast the value if it's not a JsonElement
                if (value is T castValue)
                {
                    return castValue;
                }
            }

            return default;
        }
    }


    public class DefaultEntity : BaseEntity, IHaveOwner
    {
        [YamlIgnore]
        [JsonIgnore]
        public BaseEntity? Owner { get; set; }
    }
}
