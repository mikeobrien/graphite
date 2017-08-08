using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Graphite.Extensions
{
    public static class StringExtensions
    {
        public static string RegexEscape(this string source)
        {
            return Regex.Escape(source);
        }

        public static string InitialCap(this string source)
        {
            return source.IsNotNullOrEmpty() ? (source.Substring(0, 1).ToUpper() + 
                (source.Length > 1 ? source.Substring(1).ToLower() : "")) : source;
        }

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

        public static string ToTable<T, TRow>(this IEnumerable<T> source, Expression<Func<T, TRow>> row)
        {
            var map = row.Body.As<NewExpression>();
            var sourceProperties = map.Arguments.Cast<MemberExpression>()
                .Select(x => x.Member).Cast<PropertyInfo>().ToArray();
            var columns = map.Members.Select(x => x.Name.Replace("_", " ")).ToArray();
            var data = source.Select(x => sourceProperties.Select(y => y.GetValue(x)?.ToString() ?? "").ToArray()).ToArray();
            var columnWidths = columns.Select((x, i) => Math.Max(data.Max(y => y[i].Length), x.Length)).ToArray();
            return columns.Concat(data).Select((x, r) => x.Select((y, i) => 
                y.PadRight(columnWidths[i], r % 2 == 0 ? '·' : ' ')).Join(" ")).Join("\r\n");
        }

        public static string ToBase64(this string text, Encoding encoding)
        {
            return text.IsNullOrEmpty() 
                ? "" 
                : Convert.ToBase64String(encoding.GetBytes(text));
        }

        public static string FromBase64(this string base64, Encoding encoding)
        {
            if (base64.IsNullOrEmpty()) return "";
            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(base64);
            }
            catch
            {
                return "";
            }
            return encoding.GetString(bytes);
        }
    }
}
