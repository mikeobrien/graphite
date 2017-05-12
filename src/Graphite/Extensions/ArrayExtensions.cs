using System.Collections.Generic;

namespace Graphite.Extensions
{
    public class ArrayItem<T>
    {
        public ArrayItem(T[] array, int index)
        {
            Array = array;
            Index = index;
        }

        public T[] Array { get; }
        public int Index { get; }

        public T Value
        {
            get { return Array[Index]; }
            set { Array[Index] = value; }
        }
    }

    public static class ArrayExtensions
    {
        public static ArrayItem<T> GetItem<T>(this T[] source, int index)
        {
            return new ArrayItem<T>(source, index);
        }

        public static T AddItem<T>(this IList<T> source, T item)
        {
            source.Add(item);
            return item;
        }
    }
}
