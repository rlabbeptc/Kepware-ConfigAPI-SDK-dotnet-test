using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KepwareSync
{
    public abstract class CustomHashGenerator
    {
        public class HashSourceBuilder : IEnumerable<KeyValuePair<string, object?>>
        {
            public LinkedList<KeyValuePair<string, object?>> LinkedList { get; }

            public HashSourceBuilder()
            {
                LinkedList = new LinkedList<KeyValuePair<string, object?>>();
            }
            public HashSourceBuilder(KeyValuePair<string, object?> seed)
            {
                LinkedList = new LinkedList<KeyValuePair<string, object?>>([seed]);
            }
            public HashSourceBuilder Append<T>(string key, T? value = default)
             => Append(new KeyValuePair<string, object?>(key, value));

            public HashSourceBuilder Append(KeyValuePair<string, object?> kvp)
            {
                LinkedList.AddLast(kvp);
                return this;
            }

            public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
             => LinkedList.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
             => GetEnumerator();
        }

        public static HashSourceBuilder CreateHashSourceBuilder()
        {
            return new HashSourceBuilder();
        }
        public static HashSourceBuilder CreateHashSourceBuilder<T>(string seedKey, T? seedValue = default)
         => CreateHashSourceBuilder(new KeyValuePair<string, object?>(seedKey, seedValue));

        public static HashSourceBuilder CreateHashSourceBuilder(KeyValuePair<string, object?> seed)
        {
            return new HashSourceBuilder(seed);
        }

        /// <summary>
        /// Interne Methode zur Berechnung des Hashs unter Verwendung von FNV-1a.
        /// </summary>
        /// <param name="data">Das zu hashende Dictionary.</param>
        /// <returns>Der berechnete Hashwert als long.</returns>
        public static ulong ComputeHash(IEnumerable<KeyValuePair<string, object?>> data)
        {
            const ulong fnvOffsetBasis = 14695981039346656037;
            const ulong fnvPrime = 1099511628211;

            ulong hash = fnvOffsetBasis;

            foreach (var kvp in data)
            {
                // Hash the key
                foreach (byte b in Encoding.UTF8.GetBytes(kvp.Key))
                {
                    hash ^= b;
                    hash *= fnvPrime;
                }

                // Hash the value's string representation
                string valueString = kvp.Value?.ToString() ?? string.Empty;
                foreach (byte b in Encoding.UTF8.GetBytes(valueString))
                {
                    hash ^= b;
                    hash *= fnvPrime;
                }
            }

            return hash;
        }
    }
}
