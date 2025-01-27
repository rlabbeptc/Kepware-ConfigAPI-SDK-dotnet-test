using Kepware.Api.Model;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Kepware.Api.Util
{
    public static class HelperExtentions
    {

        private static readonly FrozenSet<char> InvalidChars = new HashSet<char> { '\\', '/', ':', '*', '?', '\\', '<', '>', '|', EscapeChar }.ToFrozenSet();
        private const char EscapeChar = '%'; // Escape character for encoding

        public static IEnumerable<NamedEntity> Flatten(this NamedEntity? node)
        {
            NamedEntity? current = node;
            while (current != null)
            {
                yield return current;
                current = current.Owner;
            }
        }

        public static IEnumerable<NamedEntity> Flatten(this NamedEntity? node, Type matchingType)
        {
            NamedEntity? current = node;
            while (current != null)
            {
                yield return current;
                current = current.Owner;
            }
        }

        public static string EscapeDiskEntry(this string path)
        {
            return string.Concat(path.Select(c =>
             InvalidChars.Contains(c) ? EscapeChar + ((int)c).ToString("X2") : c.ToString()));
        }


        public static string UnescapeDiskEntry(this string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path cannot be null or empty.", nameof(path));

            // Worst-case: Länge des ursprünglichen Strings
            var result = new char[path.Length];
            int index = 0;

            for (int i = 0; i < path.Length; i++)
            {
                if (path[i] == EscapeChar && i + 2 < path.Length &&
                    IsHexDigit(path[i + 1]) && IsHexDigit(path[i + 2]))
                {
                    // Konvertiere %XX zurück in ein Zeichen
                    string hex = new string(new[] { path[i + 1], path[i + 2] });
                    result[index++] = (char)Convert.ToInt32(hex, 16);
                    i += 2; // Überspringe die beiden Hex-Zeichen
                }
                else
                {
                    // Normales Zeichen übernehmen
                    result[index++] = path[i];
                }
            }

            // Gib den tatsächlich verwendeten Teil zurück
            return new string(result, 0, index);
        }

        private static bool IsHexDigit(char c)
        {
            return c >= '0' && c <= '9' ||
                   c >= 'A' && c <= 'F' ||
                   c >= 'a' && c <= 'f';
        }


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
