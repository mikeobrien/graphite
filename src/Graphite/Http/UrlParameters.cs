using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;

namespace Graphite.Http
{
    public class UrlParameters : Dictionary<string, object>
    {
        public UrlParameters() { }
        public UrlParameters(IDictionary<string, object> source) : base(source) { }

        public static UrlParameters CreateFrom(HttpRequestContext context)
        {
            return new UrlParameters(context.RouteData.Values
                .ToDictionary(x => x.Key, x => x.Value));
        }
    }
}