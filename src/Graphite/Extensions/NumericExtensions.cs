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

        public static int TryParseInt(this object value)
        {
            if (value is int) return (int) value;
            int intValue;
            return value is string && int.TryParse(
                value.ToString(), out intValue) ? intValue : 0;
        }
    }
}
