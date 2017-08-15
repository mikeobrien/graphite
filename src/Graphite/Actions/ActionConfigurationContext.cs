using System.Web.Http;
using Graphite.Routing;

namespace Graphite.Actions
{
    public class ActionConfigurationContext
    {
        public ActionConfigurationContext(Configuration configuration, 
            HttpConfiguration httpConfiguration, ActionMethod actionMethodMethod, 
            RouteDescriptor routeDescriptorDescriptor)
        {
            Configuration = configuration;
            HttpConfiguration = httpConfiguration;
            ActionMethod = actionMethodMethod;
            RouteDescriptor = routeDescriptorDescriptor;
        }

        public Configuration Configuration { get; }
        public HttpConfiguration HttpConfiguration { get; }
        public virtual ActionMethod ActionMethod { get; }
        public virtual RouteDescriptor RouteDescriptor { get; }
    }
}