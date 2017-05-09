using System.Collections.Generic;
using System.Web.Http;
using Graphite.Actions;
using Graphite.Extensibility;

namespace Graphite.Routing
{
    public class RouteContext
    {
        public RouteContext(Configuration configuration, 
            HttpConfiguration httpConfiguration, ActionMethod actionMethod)
        {
            Configuration = configuration;
            HttpConfiguration = httpConfiguration;
            ActionMethod = actionMethod;
        }

        public virtual Configuration Configuration { get; }
        public virtual HttpConfiguration HttpConfiguration { get; }
        public virtual ActionMethod ActionMethod { get; }
    }

    public interface IRouteConvention : IConditional<RouteContext>
    {
        List<RouteDescriptor> GetRouteDescriptors(RouteContext context);
    }
}