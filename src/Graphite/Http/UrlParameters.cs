using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Web.Http.Controllers;
using Graphite.Collections;

namespace Graphite.Http
{
    public class UrlParameters : DictionaryWrapper<string, object>
    {
        public UrlParameters() : base(null) { }

        public UrlParameters(IDictionary<string, object> source) : 
            base(new ReadOnlyDictionary<string, object>(new Dictionary<string, object>(
                source, StringComparer.OrdinalIgnoreCase))) { }

        public static UrlParameters CreateFrom(HttpRequestMessage message)
        {
            return new UrlParameters(message.GetRequestContext().RouteData.Values);
        }

        public override object this[string key] => TryGetValue(key, out var value) ? value : null;
    }
}