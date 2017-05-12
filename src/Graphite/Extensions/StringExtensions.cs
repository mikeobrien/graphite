using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Graphite.Extensions
{
    public static class StringExtensions
    {
        public static bool EqualsIgnoreCase(this string source, string compare)
        {
            return source.Equals(compare, StringComparison.OrdinalIgnoreCase);
        }

        public static bool StartsWithIgnoreCase(this string source, string compare)
        {
            return source.StartsWith(compare, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool IsNotNullOrEmpty(this string value)
        {
            return !string.IsNullOrEmpty(value);
        }

        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static bool IsNotNullOrWhiteSpace(this string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        public static string Join(this IEnumerable<string> parts, string seperator = "")
        {
            return string.Join(seperator, parts);
        }

        public static string Remove(this string source, params string[] regex)
        {
            return source.Remove((IEnumerable<string>)regex);
        }

        public static string Remove(this string source, IEnumerable<string> regex)
        {
            return regex.Aggregate(source, (a, i) => Regex.Replace(a, i, ""));
        }

        public static bool IsMatch(this string source, string regex)
        {
            return Regex.IsMatch(source, regex);
        }

        public static List<string> MatchGroups(this string source, string regex)
        {
            return Regex.Match(source, regex).Groups.Cast<Group>()
                .Skip(1).Select(x => x.Value).ToList();
        }

        public static bool Contains(this string source, 
            string find, StringComparison comparison)
        {
            return source.IndexOf(find, comparison) >= 0;
        }

        public static bool ContainsIgnoreCase(this string value, params string[] find)
        {
            if (value.IsNullOrEmpty()) return false;
            return find.Where(x => x.IsNotNullOrEmpty()).Any(y => 
                value.Contains(y, StringComparison.OrdinalIgnoreCase));
        }

        public static string[] Split(this object value, char separator)
        {
            return value?.ToString().Split(separator);
        }

        public static string Truncate(this string value, int length)
        {
            return value == null 
                ? null 
                : (value.Length <= length 
                    ? "" 
                    : value.Substring(0, value.Length - length));
        }
    }
}
