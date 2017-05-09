using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using Graphite.Reflection;
using Graphite.Routing;

namespace Graphite.Actions
{
    public class RequestContext
    {
        public RequestContext(ActionMethod actionMethod, RouteDescriptor route,
           TypeDescriptor[] behaviors, IDictionary<string, string> urlParameters,
           ILookup<string, string> querystringParameters,
           HttpRequestMessage requestMessage, HttpConfiguration httpConfiguration,
           CancellationToken cancellationToken)
        {
            Action = actionMethod;
            Route = route;
            Behaviors = behaviors;
            UrlParameters = urlParameters;
            QuerystringParameters = querystringParameters;
            RequestMessage = requestMessage;
            HttpConfiguration = httpConfiguration;
            CancellationToken = cancellationToken;
        }

        public virtual ActionMethod Action { get; }
        public virtual RouteDescriptor Route { get; }
        public virtual IDictionary<string, string> UrlParameters { get; }
        public virtual ILookup<string, string> QuerystringParameters { get; }
        public virtual TypeDescriptor[] Behaviors { get; }
        public virtual HttpRequestMessage RequestMessage { get; }
        public virtual HttpConfiguration HttpConfiguration { get; }
        public virtual CancellationToken CancellationToken { get; }
    }
}
