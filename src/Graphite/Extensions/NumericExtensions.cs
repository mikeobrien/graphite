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

        public static string ToSizeString(this int bytes)
        {
            string result;
            if (bytes < 1024) result = $"{bytes}B";
            else if (bytes < Math.Pow(1024, 2)) result = $"{bytes / 1024.0:##.#}KB";
            else if (bytes < Math.Pow(1024, 3)) result = $"{bytes / Math.Pow(1024.0, 2):##.#}MB";
            else result = $"{bytes / Math.Pow(1024.0, 3):##.#}GB";
            return result.Replace(".0", "");
        }

        public static string ToSizeString(this int? bytes)
        {
            return bytes?.ToSizeString();
        }
    }
}
