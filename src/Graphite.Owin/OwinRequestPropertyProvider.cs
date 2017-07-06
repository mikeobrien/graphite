using System;
using System.Collections.Generic;
using System.Net.Http;
using Graphite.Extensions;
using Graphite.Http;
using Microsoft.Owin;

namespace Graphite.Owin
{
    public class OwinRequestPropertyProvider : IRequestPropertiesProvider
    {
        public const string OwinContextKey = "MS_OwinContext";

        private readonly Lazy<IDictionary<string, object>> _requestProperties;

        public OwinRequestPropertyProvider(HttpRequestMessage requestMessage)
        {
            _requestProperties = requestMessage.ToLazy(GetProperties);
        }

        public IDictionary<string, object> GetProperties()
        {
            return _requestProperties.Value;
        }

        private static IDictionary<string, object> GetProperties(HttpRequestMessage requestMessage)
        {
            if (!requestMessage.Properties.ContainsKey(OwinContextKey)) return null;

            var request = (requestMessage.Properties[OwinContextKey] as OwinContext)?.Request;

            if (request == null) return null;

            return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                ["RemoteAddress"] = request.RemoteIpAddress,
                ["RemotePort"] = request.RemotePort
            };
        }
    }
}
