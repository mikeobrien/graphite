using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using Graphite.Extensions;

namespace Graphite.Http
{
    public static class Extensions
    {
        public static bool HasMimeMultipartContent(this HttpRequestMessage request)
        {
            return request.Content?.IsMimeMultipartContent() == true;
        }

        public static T GetOrAddProperty<T>(this HttpRequestMessage request, Func<T> create) where T : class
        {
            var key = typeof(T).FullName;
            if (request.Properties.TryGetValue(key, out var property)) return property as T;

            var value = create();
            request.Properties[key] = value;
            return value;
        }

        public static bool IsQuoted(this string value)
        {
            value = value?.Trim();
            return value != null && value.Length > 1 &&
                value.StartsWith("\"") && value.EndsWith("\"");
        }

        public static string Unquote(this string value)
        {
            value = value?.Trim();
            return value.IsQuoted()
                ? value.Substring(1, value.Length - 2)
                : value;
        }

        public static string Quote(this string value)
        {
            return !value.IsQuoted()
                ? $"\"{value}\""
                : value;
        }

        public static IList<KeyValuePair<string, string>> ParseHeaders(this string headers)
        {
            headers = headers?.Trim();
            if (headers.IsNullOrEmpty()) return Enumerable
                .Empty<KeyValuePair<string, string>>().ToList();
            return headers.Split("\r\n", StringSplitOptions.RemoveEmptyEntries)
                .Select(ParseHeader).ToList();
        }

        public static KeyValuePair<string, string> ParseHeader(this string header)
        {
            if (header.IsNullOrEmpty()) return new KeyValuePair<string, string>("", "");
            var headerParts = header.Split(new[] { ':' }, 2);
            return new KeyValuePair<string, string>(headerParts[0].Trim(),
                headerParts.Length > 1 ? headerParts[1].Trim() : "");
        }

        public static List<string> ParseTokens(this string tokens)
        {
            if (tokens.IsNullOrEmpty()) return null;
            return tokens.Split(',').Select(x => x.Trim())
                .Where(x => x != "").ToList();
        }

        public static string GetContentBoundry(this HttpContentHeaders headers)
        {
            return headers.ContentType.Parameters
                .Where(x => x.Name.EqualsUncase("boundary"))
                .Select(x => x.Value.Unquote())
                .FirstOrDefault();
        }

        public static string UrlDecode(this string value)
        {
            return HttpUtility.UrlDecode(value);
        }        
		
		public static HttpResponseMessage SafeSetReasonPhrase(
            this HttpResponseMessage response, string reasonPhrase)
        {
            response.ReasonPhrase = reasonPhrase?
                .Replace("\r\n", " ")
                .Replace("\r", " ")
                .Replace("\n", " ")
                // This is the max length allowed by Web Api
                // although the spec doesn't define a max length.
                .Left(512); 
            return response;
        }
    }
}
