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
    }
}
