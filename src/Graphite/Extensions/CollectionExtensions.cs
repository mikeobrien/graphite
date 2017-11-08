using System;
using Graphite.Linq;
using System.Collections.Generic;

namespace Graphite.Extensions
{
    public static class CollectionExtensions
    {
        public static T AddItem<T>(this IList<T> source, T item)
        {
            source.Add(item);
            return item;
        }

        public static IList<T> AddRange<T>(this IList<T> source, params T[] items)
        {
            items.ForEach(source.Add);
            return source;
        }

        public static ICollection<T> AddRange<T>(this ICollection<T> source, 
            IEnumerable<T> items)
        {
            items.ForEach(source.Add);
            return source;
        }

        public static List<T> AddRange<T>(this List<T> list, params T[] items)
        {
            list.AddRange(items);
            return list;
        }

        public static List<Type> Add<T>(this List<Type> list)
        {
            list.Add(typeof(T));
            return list;
        }
    }
}
