using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Collections;
using System.Text.Json;
using KepwareSync.Serializer;

namespace KepwareSync.Model
{
    [JsonSerializable(typeof(Project))]
    [JsonSerializable(typeof(JsonProjectRoot))]
    [JsonSerializable(typeof(ProductInfo))]
    [JsonSerializable(typeof(List<Channel>))]
    [JsonSerializable(typeof(List<Device>))]
    [JsonSerializable(typeof(List<Tag>))]
    [JsonSerializable(typeof(List<DeviceTagGroup>))]
    [JsonSerializable(typeof(List<DefaultEntity>))]
    [JsonSerializable(typeof(List<object?>))]
    [JsonSerializable(typeof(List<ApiResult>))]
    [JsonSerializable(typeof(int))]
    [JsonSerializable(typeof(long))]
    [JsonSerializable(typeof(float))]
    [JsonSerializable(typeof(double))]
    [JsonSerializable(typeof(bool))]
    [JsonSerializable(typeof(Dictionary<string, object?>))]
    [JsonSourceGenerationOptions(WriteIndented = true)]
    public partial class KepJsonContext : JsonSerializerContext
    {
        public static JsonTypeInfo<T> GetJsonTypeInfo<T>()
          where T : BaseEntity
        {
            if (typeof(T) == typeof(Channel))
            {
                return (JsonTypeInfo<T>)(object)Default.Channel;
            }
            else if (typeof(T) == typeof(Project))
            {
                return (JsonTypeInfo<T>)(object)Default.Project;
            }
            else if (typeof(T) == typeof(Device))
            {
                return (JsonTypeInfo<T>)(object)Default.Device;
            }
            else if (typeof(T) == typeof(Tag))
            {
                return (JsonTypeInfo<T>)(object)Default.Tag;
            }
            else if (typeof(T) == typeof(DeviceTagGroup))
            {
                return (JsonTypeInfo<T>)(object)Default.DeviceTagGroup;
            }
            else if (typeof(T) == typeof(DefaultEntity))
            {
                return (JsonTypeInfo<T>)(object)Default.DefaultEntity;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static JsonTypeInfo<List<T>> GetJsonListTypeInfo<T>()
            where T : BaseEntity
        {
            if (typeof(T) == typeof(Channel))
            {
                return (JsonTypeInfo<List<T>>)(object)Default.ListChannel;
            }
            else if (typeof(T) == typeof(Device))
            {
                return (JsonTypeInfo<List<T>>)(object)Default.ListDevice;
            }
            else if (typeof(T) == typeof(DeviceTagGroup))
            {
                return (JsonTypeInfo<List<T>>)(object)Default.ListDeviceTagGroup;
            }
            else if (typeof(T) == typeof(Tag))
            {
                return (JsonTypeInfo<List<T>>)(object)Default.ListTag;
            }
            else if (typeof(T) == typeof(DefaultEntity))
            {
                return (JsonTypeInfo<List<T>>)(object)Default.ListDefaultEntity;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static IEnumerable<KeyValuePair<string, object?>> Unwrap(IEnumerable<KeyValuePair<string, JsonElement>> dic)
        {
            return dic.Select(kvp => new KeyValuePair<string, object?>(kvp.Key, Unwrap(kvp.Value)));
        }

        public static object? Unwrap(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    // Rekursive Entpackung für Objekte
                    return element.EnumerateObject()
                        .ToDictionary(prop => prop.Name, prop => Unwrap(prop.Value));

                case JsonValueKind.Array:
                    // Rekursive Entpackung für Arrays
                    return element.EnumerateArray()
                        .Select(Unwrap)
                        .ToList();

                case JsonValueKind.String:
                    return element.GetString();

                case JsonValueKind.Number:
                    if (element.TryGetInt64(out var longValue))
                    {
                        return longValue;
                    }
                    if (element.TryGetDouble(out var doubleValue))
                    {
                        return doubleValue;
                    }
                    return null; // Falls die Zahl nicht aufgelöst werden kann

                case JsonValueKind.True:
                case JsonValueKind.False:
                    return element.GetBoolean();

                case JsonValueKind.Null:
                    return null;

                default:
                    // Unbekannte oder unsupported ValueKind
                    return element.GetRawText();
            }
        }

        public static bool Equals(JsonElement element, JsonElement other)
        {
            if (element.ValueKind != other.ValueKind)
            {
                return false;
            }

            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    var elementProperties = element.EnumerateObject().ToDictionary(prop => prop.Name, prop => prop.Value);
                    var otherProperties = other.EnumerateObject().ToDictionary(prop => prop.Name, prop => prop.Value);

                    if (elementProperties.Count != otherProperties.Count)
                    {
                        return false;
                    }

                    foreach (var property in elementProperties)
                    {
                        if (!otherProperties.TryGetValue(property.Key, out var otherValue))
                        {
                            return false;
                        }

                        if (!Equals(property.Value, otherValue))
                        {
                            return false;
                        }
                    }

                    return true;

                case JsonValueKind.Array:
                    var elementArray = element.EnumerateArray().ToArray();
                    var otherArray = other.EnumerateArray().ToArray();

                    if (elementArray.Length != otherArray.Length)
                    {
                        return false;
                    }

                    for (int i = 0; i < elementArray.Length; i++)
                    {
                        if (!Equals(elementArray[i], otherArray[i]))
                        {
                            return false;
                        }
                    }

                    return true;

                case JsonValueKind.String:
                    return element.GetString() == other.GetString();

                case JsonValueKind.Number:
                    if (element.TryGetInt64(out var elementLong) && other.TryGetInt64(out var otherLong))
                    {
                        return elementLong == otherLong;
                    }

                    if (element.TryGetDouble(out var elementDouble) && other.TryGetDouble(out var otherDouble))
                    {
                        return Math.Abs(elementDouble - otherDouble) <= double.Epsilon;
                    }

                    return false;

                case JsonValueKind.True:
                case JsonValueKind.False:
                    return element.GetBoolean() == other.GetBoolean();

                case JsonValueKind.Null:
                    return true;

                default:
                    return element.GetRawText() == other.GetRawText();
            }
        }


        internal static JsonElement WrapInJsonElement(object? value)
        {
            if (value == null)
            {
                return JsonSerializer.SerializeToElement(null, Default.Object);
            }
            else if (value is bool blnValue)
            {
                return JsonSerializer.SerializeToElement(blnValue, Default.Boolean);
            }
            else if (value is int nValue)
            {
                return JsonSerializer.SerializeToElement(nValue, Default.Int32);
            }
            else if (value is long lValue)
            {
                return JsonSerializer.SerializeToElement(lValue, Default.Int64);
            }
            else if (value is float fValue)
            {
                return JsonSerializer.SerializeToElement(fValue, Default.Single);
            }
            else if (value is double dValue)
            {
                return JsonSerializer.SerializeToElement(dValue, Default.Double);
            }
            else if (value is string strValue)
            {
                return JsonSerializer.SerializeToElement(strValue, Default.String);
            }
            else if (value is Dictionary<string, object?> dict)
            {
                return JsonSerializer.SerializeToElement(dict, Default.DictionaryStringObject);
            }
            else if (value is List<object?> list)
            {
                return JsonSerializer.SerializeToElement(list, Default.ListObject);
            }
            else if (value is BaseEntity entity)
            {
                return JsonSerializer.SerializeToElement(entity, GetJsonTypeInfo<BaseEntity>());
            }
            else if (value is IEnumerable<BaseEntity> entities)
            {
                return JsonSerializer.SerializeToElement(entities, GetJsonListTypeInfo<BaseEntity>());
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
