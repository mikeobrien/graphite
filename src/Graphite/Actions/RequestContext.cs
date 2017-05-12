using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using Graphite.Http;
using Graphite.Reflection;
using Graphite.Routing;

namespace Graphite.Actions
{
    public class RequestContext
    {
        public RequestContext(ActionMethod actionMethod, RouteDescriptor route,
           TypeDescriptor[] behaviors, UrlParameters urlParameters,
           QuerystringParameters querystringParameters,
           HttpRequestMessage requestMessage, HttpConfiguration httpConfiguration,
           HttpRequestContext httpRequestContext, CancellationToken cancellationToken)
        {
            Action = actionMethod;
            Route = route;
            Behaviors = behaviors;
            UrlParameters = urlParameters;
            QuerystringParameters = querystringParameters;
            RequestMessage = requestMessage;
            HttpConfiguration = httpConfiguration;
            HttpRequestContext = httpRequestContext;
            CancellationToken = cancellationToken;
        }

        public virtual ActionMethod Action { get; }
        public virtual RouteDescriptor Route { get; }
        public virtual UrlParameters UrlParameters { get; }
        public virtual QuerystringParameters QuerystringParameters { get; }
        public virtual TypeDescriptor[] Behaviors { get; }
        public virtual HttpRequestMessage RequestMessage { get; }
        public virtual HttpConfiguration HttpConfiguration { get; }
        public virtual HttpRequestContext HttpRequestContext { get; }
        public virtual CancellationToken CancellationToken { get; }
    }
}
