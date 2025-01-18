using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KepwareSync
{
    public static class HelperExtentions
    {
        public static IEnumerable<KeyValuePair<K, V>> Except<K, V>(this IDictionary<K, V> source, ISet<K> except, ISet<K>? except2 = default, ISet<K>? except3 = default)
        {
            if (except2 == null)
            {
                return source.Where(x => !except.Contains(x.Key));
            }
            else if (except3 == null)
            {
                return source.Where(x => !except.Contains(x.Key) && !except2.Contains(x.Key));
            }
            else
            {
                return source.Where(x => !except.Contains(x.Key) && !except2.Contains(x.Key) && !except3.Contains(x.Key));
            }
        }

        public static T? GetValue<T>(this IDictionary<string, object?> source, string key)
        {
            if (source.TryGetValue(key, out var value))
            {
                if (value is T t)
                {
                    return t;
                }
                else if (typeof(T) == typeof(string))
                {
                    return (T)(object)(value?.ToString() ?? string.Empty);
                }
                else if (value is string sValue && string.IsNullOrEmpty(sValue))
                {
                    return default;
                }
                else if (typeof(T) == typeof(int))
                {
                    return (T)(object)Convert.ToInt32(value ?? default(int));
                }
                else if (typeof(T) == typeof(double))
                {
                    return (T)(object)Convert.ToDouble(value ?? default(double));
                }
                else if (typeof(T) == typeof(bool))
                {
                    return (T)(object)Convert.ToBoolean(value ?? default(bool));
                }
                else if (typeof(T) == typeof(DateTime))
                {
                    return (T)(object)Convert.ToDateTime(value ?? default(DateTime));
                }
                else if (typeof(T).IsEnum)
                {
                    return (T)Enum.Parse(typeof(T), value?.ToString() ?? throw new InvalidCastException(), true);
                }
            }

            throw new KeyNotFoundException($"Key '{key}' not found in the dictionary.");
        }
    }
}
