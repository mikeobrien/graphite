using System.Web.Http;
using Graphite.Actions;
using Graphite.Routing;

namespace Graphite.Binding
{
    public class ValueMapperConfigurationContext : ValueMapperContext
    {
        public ValueMapperConfigurationContext(ConfigurationContext configurationContext,
            ActionMethod actionMethod, RouteDescriptor routeDescriptor, 
            ActionParameter parameter, object[] values)
            : base(parameter, values)
        {
            Configuration = configurationContext.Configuration;
            HttpConfiguration = configurationContext.HttpConfiguration;
            Action = actionMethod;
            Route = routeDescriptor;
        }

        public Configuration Configuration { get; }
        public HttpConfiguration HttpConfiguration { get; }
        public virtual ActionMethod Action { get; }
        public virtual RouteDescriptor Route { get; }
    }
}