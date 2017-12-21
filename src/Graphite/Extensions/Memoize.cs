using System;
using System.Collections.Concurrent;

namespace Graphite.Extensions
{
    public static class Memoize
    {
        public static Func<TKey, TResult> Func<TKey, TResult>(this Func<TKey, TResult> func)
        {
            var map = new ConcurrentDictionary<TKey, TResult>();
            return x =>
            {
                if (map.ContainsKey(x)) return map[x];
                var result = func(x);
                map.TryAdd(x, result);
                return result; 
            };
        }

        public static Func<TKey, TArg, TResult> Func<TKey, TArg, TResult>(this Func<TKey, TArg, TResult> func)
        {
            var map = new ConcurrentDictionary<TKey, TResult>();
            return (k, a) =>
            {
                if (map.ContainsKey(k)) return map[k];
                var result = func(k, a);
                map.TryAdd(k, result);
                return result;
            };
        }

        public static Func<TKey, TArg1, TArg2, TResult> Func<TKey, TArg1, TArg2, TResult>(
            this Func<TArg1, TArg2, TResult> func)
        {
            var map = new ConcurrentDictionary<TKey, TResult>();
            return (k, a1, a2) =>
            {
                if (map.ContainsKey(k)) return map[k];
                var result = func(a1, a2);
                map.TryAdd(k, result);
                return result;
            };
        }

        public static Func<TKey, TArg1, TArg2, TResult> Func<TKey, TArg1, TArg2, TResult>(
            this Func<TKey, TArg1, TArg2, TResult> func)
        {
            var map = new ConcurrentDictionary<TKey, TResult>();
            return (k, a1, a2) =>
            {
                if (map.ContainsKey(k)) return map[k];
                var result = func(k, a1, a2);
                map.TryAdd(k, result);
                return result;
            };
        }
    }
}
