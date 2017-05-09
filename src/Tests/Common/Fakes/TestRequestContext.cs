using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using Graphite.Actions;
using Graphite.Reflection;
using Graphite.Routing;

namespace Tests.Common.Fakes
{
    public class TestRequestContext : RequestContext
    {
        public TestRequestContext(ActionMethod actionMethod = null, 
            RouteDescriptor route = null, TypeDescriptor[] behaviors = null, 
            IDictionary<string, string> urlParameters = null, 
            ILookup<string, string> querystringParameters = null, 
            HttpRequestMessage requestMessage = null, 
            HttpConfiguration httpConfiguration = null, 
            CancellationToken cancellationToken = new CancellationToken()) : 
            base(actionMethod, route, behaviors, urlParameters, 
                querystringParameters, requestMessage, 
                httpConfiguration, cancellationToken) { }
    }
}
