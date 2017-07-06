using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web;
using Graphite.Extensions;
using Graphite.Http;

namespace Graphite.AspNet
{
    public class AspNetRequestPropertyProvider : IRequestPropertiesProvider
    {
        public const string HttpContextKey = "MS_HttpContext";

        private readonly Lazy<IDictionary<string, object>> _requestProperties;

        public AspNetRequestPropertyProvider(HttpRequestMessage requestMessage)
        {
            _requestProperties = requestMessage.ToLazy(GetProperties);
        }

        public IDictionary<string, object> GetProperties()
        {
            return _requestProperties.Value;
        }

        private static IDictionary<string, object> GetProperties(HttpRequestMessage requestMessage)
        {
            return requestMessage.Properties.ContainsKey(HttpContextKey)
                ? requestMessage.Properties[HttpContextKey].As<HttpContextBase>()?
                    .Request?.ServerVariables?.ToDictionary<object>(
                        NormalizeServerVariableKey, x => x) : null;
        }

        private static string NormalizeServerVariableKey(string key)
        {
            return key.Replace("_ADDR", "ADDRESS").Replace("_", "");
        }
    }
}
