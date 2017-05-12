using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using Graphite.Actions;
using Graphite.Http;
using Graphite.Reflection;
using Graphite.Routing;

namespace Tests.Common.Fakes
{
    public class TestRequestContext : RequestContext
    {
        public TestRequestContext(ActionMethod actionMethod = null, 
            RouteDescriptor route = null, TypeDescriptor[] behaviors = null, 
            UrlParameters urlParameters = null, 
            QuerystringParameters querystringParameters = null, 
            HttpRequestMessage requestMessage = null, 
            HttpConfiguration httpConfiguration = null,
            HttpRequestContext httpRequestContext = null,
            CancellationToken cancellationToken = new CancellationToken()) : 
            base(actionMethod, route, behaviors, urlParameters, 
                querystringParameters, requestMessage, 
                httpConfiguration, httpRequestContext, 
                cancellationToken) { }
    }
}
