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
    }
}
