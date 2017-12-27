using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using Graphite.Http;
using Graphite.Linq;

namespace Graphite.Extensions
{
    public enum MatchType
    {
        None = 0,
        Full = 1,
        Partial = 2,
        Any = 3
    }

    public class AcceptTypeMatch
    {
        public AcceptTypeMatch(MatchType matchType, string contentType, string acceptType, double quality)
        {
            MatchType = matchType;
            ContentType = contentType;
            AcceptType = acceptType;
            Quality = quality;
        }

        public MatchType MatchType { get; }
        public string ContentType { get; set; }
        public string AcceptType { get; }
        public double Quality { get; set; }
    }

    public static class WebApiExtensions
    {
        public static string RawHeaders(this HttpRequestMessage request)
        {
            return $"{request.Method} {request.RequestUri?.AbsolutePath} HTTP/{request.Version}\r\n" +
                request.Headers.Concat((request.Content?.Headers).OrEmpty())
                    .Select(x => $"{x.Key}: {x.Value.Join("; ")}").Join("\r\n");
        }

        public static bool IsPost(this string method)
        {
            return method.EqualsUncase("POST");
        }

        public static bool IsPut(this string method)
        {
            return method.EqualsUncase("PUT");
        }

        public static bool IsDelete(this string method)
        {
            return method.EqualsUncase("DELETE");
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

        public static double GetWeight(this AcceptTypeMatch match)
        {
            return match.Quality - ((int)match.MatchType - 1).Max(0) * .0001;
        }

        public static List<AcceptTypeMatch> GetMatchingAcceptTypes(this HttpRequestMessage request, 
            params string[] mimeTypes)
        {
            if (!mimeTypes.Any())
                throw new GraphiteException("No mime types specified.");
            return request.Headers.Accept?
                .SelectMany(x => mimeTypes.Select(m => new
                {
                    AcceptType = x.MediaType,
                    ContentType = m,
                    Match = m.MatchesAcceptType(x.MediaType),
                    Quality = x.Quality ?? 1
                }))
                .Where(x => x.Match != MatchType.None)
                .OrderByDescending(x => x.Quality)
                .ThenBy(x => x.Match)
                .Select(x => new AcceptTypeMatch(x.Match, x.ContentType, x.AcceptType, x.Quality)).ToList();
        }

        public static MatchType MatchesAcceptType(this string mimeType, string acceptMimeType)
        {
            if (mimeType.IsNullOrEmpty() || acceptMimeType.IsNullOrEmpty()) return MatchType.None;
            if (acceptMimeType == "*/*") return MatchType.Any;
            if (acceptMimeType.EndsWith("/*"))
            {
                return mimeType.StartsWithUncase(acceptMimeType
                        .Substring(0, acceptMimeType.Length - 1)) 
                    ? MatchType.Partial : MatchType.None;
            }
            return mimeType.EqualsUncase(acceptMimeType) ? MatchType.Full : MatchType.None;
        }

        public static bool ContentTypeIs(this HttpRequestMessage request, params string[] mimeTypes)
        {
            return request.Content.Headers.ContentType?.MediaType.ContentTypeIs(mimeTypes) ?? false;
        }

        public static bool ContentTypeIs(this string contentType, params string[] mimeTypes)
        {
            return contentType.IsNotNullOrEmpty() && mimeTypes.ContainsUncase(contentType);
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
                    FileName = filename.Unquote().Replace("\"", "").Quote()
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

        public static HttpResponseHeaders SetCookie(this HttpResponseHeaders headers, 
            string name, string value)
        {
            headers.AddCookies(new [] { new CookieHeaderValue(name, value) });
            return headers;
        }

        public static string GetHeaderValue(this HttpRequestMessage request, string name)
        {
            return request.GetHeaderValues(name).FirstOrDefault();
        }

        public static IEnumerable<string> GetHeaderValues(this HttpRequestMessage request, string name)
        {
            IEnumerable<string> headerValues;
            request.Headers.TryGetValues(name, out headerValues);
            return headerValues ?? Enumerable.Empty<string>();
        }

        public static void Replace<T>(this ServicesContainer container, T instance)
        {
            container.Replace(typeof(T), instance);
        }

        public static void Replace<TService, TReplacement>(this 
            ServicesContainer container) where TReplacement : TService, new()
        {
            container.Replace<TService>(new TReplacement());
        }

        public static void Add<TService, TConcrete>(this
            ServicesContainer container) where TConcrete : TService, new()
        {
            container.Replace<TService>(new TConcrete());
        }
    }
}
