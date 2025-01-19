using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NodeTypeResolvers;

namespace KepwareSync.Model
{
    public interface IHaveOwner
    {
        [JsonIgnore]
        [YamlIgnore]
        public NamedEntity? Owner { get; internal set; }
    }
    public interface IHaveName
    {
        public string Name { get; }
    }



    [DebuggerDisplay("{TypeName} - {Description}")]
    public abstract class BaseEntity : IEquatable<BaseEntity>
    {
        private ulong? _hash;

        [JsonIgnore]
        [YamlIgnore]
        public ulong Hash => _hash ??= CalculateHash();

        [JsonPropertyName(Properties.ProjectId)]
        [YamlIgnore]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? ProjectId { get; set; } = null;

        [JsonPropertyName(Properties.Description)]
        [YamlMember(Alias = Properties.Description)]
        public string? Description { get; set; } = string.Empty;

        [JsonIgnore]
        [YamlIgnore]
        public string TypeName => GetType().Name;

        [JsonExtensionData]
        // Add the known scalar types to for the Serializer as Attribte like , boolean int to support AOT

        //Yaml-Properties
        public Dictionary<string, JsonElement> DynamicProperties { get; set; } = [];

        public virtual bool Equals(BaseEntity? other)
        {
            return other?.Hash == Hash;
        }

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

        public void SetDynamicProperty<T>(string key, T value)
        {
            if (value is JsonElement jsonElement)
            {
                DynamicProperties[key] = jsonElement;
            }
            else
            {
                DynamicProperties[key] = KepJsonContext.WrapInJsonElement(value);
            }
            _hash = null;
        }

        protected internal virtual ulong CalculateHash()
        {
            return CustomHashGenerator.ComputeHash(
                    KepJsonContext.Unwrap(DynamicProperties.Except(Properties.NonSerialized.AsHashSet, Properties.NonUpdatable.AsHashSet, ConditionalNonSerialized()))
                        .Concat(
                            AppendHashSources(
                                CustomHashGenerator.CreateHashSourceBuilder(nameof(Description), Description)
                            )
                        )
                );
        }

        protected virtual CustomHashGenerator.HashSourceBuilder AppendHashSources(CustomHashGenerator.HashSourceBuilder builder)
        {
            return builder;
        }

        protected virtual ISet<string>? ConditionalNonSerialized()
        {
            return null;
        }
    }


    public class DefaultEntity : BaseEntity, IHaveOwner
    {
        [YamlIgnore]
        [JsonIgnore]
        public NamedEntity? Owner { get; set; }
    }

    [DebuggerDisplay("{TypeName} - {Name} - {Description}")]
    public class NamedEntity : DefaultEntity, IHaveName
    {
        [JsonPropertyName(Properties.Name)]
        [YamlIgnore]
        //Yaml File-Name
        public string Name { get; set; } = string.Empty;

        protected override CustomHashGenerator.HashSourceBuilder AppendHashSources(CustomHashGenerator.HashSourceBuilder builder)
         => base.AppendHashSources(builder).Append(nameof(Name), Name);

        public virtual Dictionary<string, JsonElement> GetUpdateDiff(NamedEntity other)
        {
            var diff = new Dictionary<string, JsonElement>();
            if (Name != other.Name)
            {
                diff[Properties.Name] = KepJsonContext.WrapInJsonElement(Name);
            }

            if (Description != other.Description)
            {
                diff[Properties.Description] = KepJsonContext.WrapInJsonElement(Description);
            }

            if (ProjectId != 0)
            {
                diff[Properties.ProjectId] = KepJsonContext.WrapInJsonElement(ProjectId);
            }

            foreach (var kvp in DynamicProperties.Except(Properties.NonSerialized.AsHashSet, Properties.NonUpdatable.AsHashSet, ConditionalNonSerialized()))
            {
                if (!other.DynamicProperties.TryGetValue(kvp.Key, out var otherValue) ||
                    !KepJsonContext.Equals(kvp.Value, otherValue))
                {
                    diff[kvp.Key] = kvp.Value;
                }
            }

            return diff;
        }
    }

    public abstract class NamedUidEntity : NamedEntity
    {
        [JsonIgnore]
        [YamlIgnore]
        public long UniqueId => GetDynamicProperty<long>(UniqueIdKey);

        protected abstract string UniqueIdKey { get; }
    }
}
