using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Graphite.Extensions
{
    public static class LinqExtensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            return source == null || !source.Any();
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source) action(item);
        }

        public static IEnumerable<T> Join<T>(this T item, IEnumerable<T> tail)
        {
            return new[] { item }.Concat(tail);
        }

        public static bool ContainsIgnoreCase(this IEnumerable<string> source,
            params string[] search)
        {
            return source.Any(x => search.Any(x.EqualsIgnoreCase));
        }

        public static List<T> AsList<T>(this T source, params T[] tail)
        {
            return source.AsList((IEnumerable<T>)tail);
        }

        public static List<T> AsList<T>(this T source, IEnumerable<T> tail)
        {
            return source.Join(tail).ToList();
        }

        public static T[] AsArray<T>(this T source, params T[] tail)
        {
            return source.AsArray((IEnumerable<T>)tail);
        }

        public static T[] AsArray<T>(this T source, IEnumerable<T> tail)
        {
            return source.Join(tail).ToArray();
        }

        public static TValue TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key)
        {
            return source.ContainsKey(key) ? source[key] : default(TValue);
        }

        public static IEnumerable<T> Append<T>(this IEnumerable<T> source, T item)
        {
            return source.Concat(new List<T> { item });
        }

        public static ILookup<string, object> ToObjectLookup(this NameValueCollection source)
        {
            return source.ToLookup<object>();
        }

        public static ILookup<string, string> ToStringLookup(this NameValueCollection source)
        {
            return source.ToLookup<string>();
        }

        public static ILookup<TKey, TValue> ToLookup<TKey, TValue>(
            this IEnumerable<KeyValuePair<TKey, TValue>> source)
        {
            return source.ToLookup(x => x.Key, x => x.Value);
        }

        private static ILookup<string, T> ToLookup<T>(this NameValueCollection source)
        {
            return source.AllKeys.SelectMany(key => source.GetValues(key)
                .Select(value => new KeyValuePair<string, T>(key, (T)(object)value)))
                .ToLookup(x => x.Key, x => x.Value);
        }
    }
}
