using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Graphite.Extensions;

namespace Graphite.Linq
{
    public static class LinqExtensions
    {
        public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }

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

        public static IEnumerable<T> Append<T>(this IEnumerable<T> source, T item)
        {
            return source.Concat(new List<T> { item });
        }

        public static IEnumerable<T> Concat<T>(this T item, IEnumerable<T> tail)
        {
            return new List<T> { item }.Concat(tail);
        }

        public static List<T> AsList<T>(this T source, IEnumerable<T> tail)
        {
            return source.Join(tail).ToList();
        }

        public static IEnumerable<T> AnyOrDefault<T>(
            this IEnumerable<T> source, Func<T> getDefault)
        {
            if (source.Any()) return source;
            var @default = getDefault();
            return @default != null ? @default.AsList() : Enumerable.Empty<T>();
        }

        public static TValue GetValue<TKey, TValue>(this ILookup<TKey, TValue> lookup, TKey key)
        {
            return lookup[key].FirstOrDefault();
        }

        public static ILookup<string, object> ToLookup(this NameValueCollection source,
            Func<string, string> getKey = null)
        {
            return source.ToLookup<object>(getKey);
        }

        public static ILookup<TKey, TValue> ToLookup<TKey, TValue>(
            this IEnumerable<KeyValuePair<TKey, TValue>> source)
        {
            return source?.ToLookup(x => x.Key, x => x.Value);
        }

        private static ILookup<string, TValue> ToLookup<TValue>(this NameValueCollection source, 
            Func<string, string> getKey = null)
        {
            return source.AllKeys.SelectMany(key => source.GetValues(key)
                .Select(value => new KeyValuePair<string, TValue>(key, (TValue)(object)value)))
                .ToLookup(x => getKey?.Invoke(x.Key) ?? x.Key, x => x.Value);
        }

        public static IEnumerable<KeyValuePair<TKey, object>> ToKeyValuePairs<TKey>(
            this IEnumerable<object> source, TKey key)
        {
            return source.Select(x => new KeyValuePair<TKey, object>(key, x));
        }

        public static IEnumerable<KeyValuePair<TKey, TValue>> ToKeyValuePairs<TSource, TKey, TValue>(
            this IEnumerable<TSource> source, Func<TSource, TKey> key, Func<TSource, TValue> value)
        {
            return source.Select(x => new KeyValuePair<TKey, TValue>(key(x), value(x)));
        }

        public static IEnumerable<TResult> JoinIgnoreCase<TOuter, TInner, TResult>(
            this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, 
            Func<TOuter, string> outerKeySelector, Func<TInner, string> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector)
        {
            return outer.Join(inner, outerKeySelector, innerKeySelector, 
                resultSelector, StringComparer.OrdinalIgnoreCase);
        }

        public static IEnumerable<T> Enumerate<T>(this T source, Func<T, T> map) where T : class
        {
            var current = source;
            while (current != null)
            {
                yield return current;
                current = map(current);
            }
        }
    }
}
