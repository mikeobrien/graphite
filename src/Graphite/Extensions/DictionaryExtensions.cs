using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Graphite.Extensions
{
    public static class DictionaryExtensions
    {
        public static TValue TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key)
        {
            return source.ContainsKey(key) ? source[key] : default(TValue);
        }

        public static Dictionary<string, TValue> ToDictionary<TValue>(this NameValueCollection source,
            Func<string, string> key = null, Func<string, TValue> value = null) where TValue : class
        {
            return source.AllKeys.ToDictionary(key ?? (x => x),
                x => value?.Invoke(source[(string) x]) ?? (TValue)(object)source[(string) x],
                StringComparer.OrdinalIgnoreCase);
        }

        public static void Add<TKey, TValue>(this IList<KeyValuePair<TKey, TValue>> source, 
            TKey key, TValue value)
        {
            source.Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        public static ILookup<TKey, TValue> ToLookup<TKey, TValue>(
            this IList<KeyValuePair<TKey, TValue>> source)
        {
            return source.ToLookup(x => x.Key, x => x.Value);
        }
    }
}