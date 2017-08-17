using System;
using System.Collections.Generic;

namespace Graphite.Extensions
{
    public static class ListExtensions
    {
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
