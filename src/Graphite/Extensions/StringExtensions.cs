using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Graphite.Linq;

namespace Graphite.Extensions
{
    public static class StringExtensions
    {
        public static string RegexEscape(this string source)
        {
            return Regex.Escape(source);
        }

        public static string[] Split(this string source, params string[] delmiter)
        {
            return source.Split(delmiter, StringSplitOptions.None);
        }

        public static string[] Split(this string source, string seperator, StringSplitOptions options)
        {
            return source.Split(new [] { seperator }, options);
        }

        public static StringBuilder AppendLine(this StringBuilder source, object value)
        {
            source.AppendLine(value.ToString());
            return source;
        }

        public static string InitialCap(this string source)
        {
            return source.IsNotNullOrEmpty() ? (source.Substring(0, 1).ToUpper() + 
                (source.Length > 1 ? source.Substring(1).ToLower() : "")) : source;
        }

        public static string PadCenter(this string value, int width, char padChar)
        {
            if (value.IsNullOrEmpty() || value.Length >= width) return value;
            var padding = new string(padChar, (int)Math
                .Ceiling((width - value.Length) / 2d));
            return $"{padding}{value}{padding}".Substring(0, width);
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
            return String.IsNullOrEmpty(value);
        }

        public static bool IsNotNullOrEmpty(this string value)
        {
            return !String.IsNullOrEmpty(value);
        }

        public static bool IsNullOrWhiteSpace(this string value)
        {
            return String.IsNullOrWhiteSpace(value);
        }

        public static bool IsNotNullOrWhiteSpace(this string value)
        {
            return !String.IsNullOrWhiteSpace(value);
        }

        public static string Join(this IEnumerable<object> parts, string seperator = "")
        {
            return Join(parts.Select(x => x?.ToString()), seperator);
        }

        public static string Join(this IEnumerable<string> parts, string seperator = "")
        {
            return string.Join(seperator, parts);
        }

        public static string Join(this IEnumerable<string> parts, char seperator)
        {
            return string.Join(seperator.ToString(), parts);
        }

        public static bool MatchesGroup(this string source, Regex regex, string name)
        {
            return regex.Match(source).Groups[name].Success;
        }

        public static string MatchGroupValue(this string source, Regex regex, string name)
        {
            if (!regex.GetGroupNames().Contains(name))
                throw new GraphiteException(
                    $"Regex '{regex}' does not contain capture group '{name}'.");
            var match = regex.Match(source);
            return match.Success ? match.Groups[name].Value : null;
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

        public static string Hash(this string value)
        {
            using (var hash = MD5.Create())
                return hash.ComputeHash(Encoding.Unicode
                    .GetBytes(value)).ToHex();
        }

        public static string ToTable<T, TRow>(this IEnumerable<T> source, Expression<Func<T, TRow>> row)
        {
            var map = row.Body.As<NewExpression>();
            var sourceProperties = map.Arguments.Cast<MemberExpression>()
                .Select(x => x.Member).Cast<PropertyInfo>().ToArray();
            var columns = map.Members.Select(x => x.Name.Replace("_", " ")).ToArray();
            var data = source.Select(x => sourceProperties
                .Select(y => y.GetValue(x)?.ToString() ?? "").ToArray()).ToArray();
            var columnWidths = columns.Select((x, i) => 
                Math.Max(data.Any() ? data.Max(y => y[i].Length) : 0, x.Length)).ToArray();
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

        public static string Repeat(this char value, int length)
        {
            return new string(value, Math.Max(length, 0));
        }

        public static string ToHierarchy(this IEnumerable<string> source)
        {
            return source.IsNullOrEmpty() ? null : 
                source.Select((x, i) => ' '.Repeat(i - 1) + 
                    (i > 0 ? "└" : "") + x.Trim())
                    .Join(Environment.NewLine);
        }

        public static IEnumerable<string> Trim(this IEnumerable<string> source)
        {
            return source.Select(x => x?.Trim());
        }
    }
}
