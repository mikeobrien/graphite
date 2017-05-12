using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Http.Dependencies;

namespace Graphite.Extensions
{
    public static class WebApiExtensions
    {
        public static bool IsPost(this string method)
        {
            return method.EqualsIgnoreCase("POST");
        }

        public static bool IsPut(this string method)
        {
            return method.EqualsIgnoreCase("PUT");
        }

        public static bool IsDelete(this string method)
        {
            return method.EqualsIgnoreCase("DELETE");
        }

        public static ILookup<string, object> ParseQueryString(this string querystring)
        {
            return HttpUtility.ParseQueryString(querystring).ToLookup();
        }

        public static T GetService<T>(this IDependencyResolver dependencyResolver)
        {
            return (T)dependencyResolver.GetService(typeof(T));
        }

        public static T GetServiceAs<T>(this IDependencyResolver dependencyResolver, Type type)
        {
            return (T)dependencyResolver.GetService(type);
        }

        public static HttpResponseMessage CreateTextResponse(this string content)
        {
            return content.CreateResponseTask("text/plain");
        }

        public static HttpResponseMessage CreateHtmlResponse(this string content)
        {
            return content.CreateResponseTask("text/html");
        }

        public static HttpResponseMessage CreateJsonResponse(this string content)
        {
            return content.CreateResponseTask("application/json");
        }

        public static HttpResponseMessage CreateResponseTask(
            this string content, string contentType)
        {
            return new HttpResponseMessage
            {
                Content = new StringContent(content, Encoding.UTF8, contentType)
            };
        }

        public static bool AcceptsMimeType(this HttpRequestMessage request, params string[] mimeTypes)
        {
            return request.Headers.Accept?.Any(x => mimeTypes
                .Any(m => m.MatchesAcceptMimeType(x.MediaType))) ?? false;
        }

        public static bool MatchesAcceptMimeType(this string mimeType, string acceptMimeType)
        {
            if (mimeType.IsNullOrEmpty() || acceptMimeType.IsNullOrEmpty()) return false;
            if (acceptMimeType.EndsWith("/*"))
            {
                return acceptMimeType == "*/*" || mimeType.StartsWithIgnoreCase(
                    acceptMimeType.Substring(0, acceptMimeType.Length - 1));
            }
            return mimeType.EqualsIgnoreCase(acceptMimeType);
        }

        public static bool ContentTypeIs(this HttpRequestMessage request, params string[] mimeTypes)
        {
            return mimeTypes.ContainsIgnoreCase(request.Content.Headers.ContentType?.MediaType);
        }

        public static string HtmlEncode(this string text)
        {
            return HttpUtility.HtmlEncode(text);
        }

        public static void SetContentType(this HttpContentHeaders headers, string mimetype)
        {
            headers.ContentType = new MediaTypeHeaderValue(mimetype);
        }

        public static void SetAttachmentDisposition(this HttpContentHeaders headers, string filename)
        {
            headers.ContentDisposition =
                new ContentDispositionHeaderValue("attachment")
                {
                    FileName = filename
                };
        }
        
        public static ILookup<string, object> ToLookup(this IEnumerable<CookieHeaderValue> cookies)
        {
            return cookies.SelectMany(c => c.Cookies.Select(x =>
                new KeyValuePair<string, object>(x.Name, x.Value))).ToLookup();
        }

        public static ILookup<string, object> ToLookup(this HttpRequestHeaders headers)
        {
            return headers.SelectMany(h => h.Value.Select(x =>
                new KeyValuePair<string, object>(h.Key, x))).ToLookup();
        }
    }
}
