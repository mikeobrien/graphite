using System;
using System.Linq;

namespace Graphite.Extensions
{
    public static class Extensions
    {
        public static string AssertNotEmptyOrWhitespace(
            this string source, string message)
        {
            if (source.IsNullOrWhiteSpace())
                throw new InvalidOperationException(message);
            return source;
        }

        public static int GetHashCode(this object source, params object[] values)
        {
            unchecked
            {
                return values.Aggregate((int)2166136261, (a, i) => 
                    a * 16777619 ^ i?.GetHashCode() ?? a);
            }
        }

        public static T CastTo<T>(this object value)
        {
            return (T) value;
        }

        public static T As<T>(this object value) where T : class
        {
            return value as T;
        }

        public static Lazy<TResult> ToLazy<T, TResult>(this T source, 
            Func<T, TResult> factory)
        {
            return new Lazy<TResult>(() => factory(source));
        }
    }
}
