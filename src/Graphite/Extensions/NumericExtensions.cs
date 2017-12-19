using System;

namespace Graphite.Extensions
{
    public static class NumericExtensions
    {
        public static int Min(this int value, int min)
        {
            return Math.Min(value, min);
        }

        public static int Max(this int value, int min)
        {
            return Math.Max(value, min);
        }

        public static int TryParseInt32(this object value)
        {
            if (value is int) return (int) value;
            int intValue;
            return value is string && int.TryParse(
                value.ToString(), out intValue) ? intValue : 0;
        }

        public static bool TryParseInt64(this string value, out long result)
        {
            return long.TryParse(value, out result);
        }

        public static int KB(this int value)
        {
            return value * 1024;
        }

        public static int MB(this int value)
        {
            return value * 1024 * 1024;
        }
    }
}
