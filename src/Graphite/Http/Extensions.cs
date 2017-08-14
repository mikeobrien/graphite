using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Graphite.Http
{
    public static class Extensions
    {
        public static QuerystringParameters Querystring(this HttpRequestMessage request)
        {
            return request.GetOrAddProperty(() => QuerystringParameters.CreateFrom(request));
        }

        public static IEnumerable<object> Querystring(this HttpRequestMessage request, string name)
        {
            return request.Querystring()[name];
        }

        public static UrlParameters UrlParameters(this HttpRequestMessage request)
        {
            return request.GetOrAddProperty(() => Http.UrlParameters.CreateFrom(request));
        }

        public static object UrlParameters(this HttpRequestMessage request, string name)
        {
            return request.UrlParameters()[name];
        }

        public static T GetOrAddProperty<T>(this HttpRequestMessage request, Func<T> create) where T : class
        {
            var key = typeof(T).FullName;
            if (request.Properties.TryGetValue(key, out var property)) return property as T;

            var value = create();
            request.Properties[key] = value;
            return value;
        }
    }
}
