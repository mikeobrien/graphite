using System;

namespace Graphite.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime ToLocalDate(this DateTime datetime)
        {
            return new DateTime(datetime.Year, datetime.Month, datetime.Day, 0, 0, 0, DateTimeKind.Local);
        }
    }
}
