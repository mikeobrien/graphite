using System.Web.Http;
using Graphite.Actions;
using Graphite.Routing;

namespace Graphite.Binding
{
    public class ValueMapperConfigurationContext : ValueMapperContext
    {
        public ValueMapperConfigurationContext(Configuration configuration,
            HttpConfiguration httpConfiguration,
            ActionMethod actionMethod, RouteDescriptor routeDescriptor, 
            ActionParameter parameter, object[] values)
            : base(parameter, values)
        {
            Configuration = configuration;
            HttpConfiguration = httpConfiguration;
            Action = actionMethod;
            Route = routeDescriptor;
        }

        public Configuration Configuration { get; }
        public HttpConfiguration HttpConfiguration { get; }
        public virtual ActionMethod Action { get; }
        public virtual RouteDescriptor Route { get; }
    }
}