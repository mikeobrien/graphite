using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using Graphite.Linq;

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

        public static string ToHex(this byte[] bytes)
        {
            return new SoapHexBinary(bytes).ToString();
        }

        public static T[] AsArray<T>(this T source, params T[] tail)
        {
            return source.AsArray((IEnumerable<T>)tail);
        }

        public static T[] AsArray<T>(this T source, IEnumerable<T> tail)
        {
            return source.Join(tail).ToArray();
        }

        public static List<T> AsList<T>(this T source, params T[] tail)
        {
            return source.AsList((IEnumerable<T>)tail);
        }
    }
}
