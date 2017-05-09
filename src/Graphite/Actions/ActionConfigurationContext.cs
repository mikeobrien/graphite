using System.Web.Http;
using Graphite.Routing;

namespace Graphite.Actions
{
    public class ActionConfigurationContext
    {
        public ActionConfigurationContext(ConfigurationContext configurationContext,
            ActionMethod actionMethodMethod, RouteDescriptor routeDescriptorDescriptor)
        {
            Configuration = configurationContext.Configuration;
            HttpConfiguration = configurationContext.HttpConfiguration;
            ActionMethod = actionMethodMethod;
            RouteDescriptor = routeDescriptorDescriptor;
        }

        public ActionConfigurationContext(ConfigurationContext configurationContext, 
            ActionDescriptor actionDescriptor)
        {
            Configuration = configurationContext.Configuration;
            HttpConfiguration = configurationContext.HttpConfiguration;
            ActionMethod = actionDescriptor.Action;
            RouteDescriptor = actionDescriptor.Route;
        }

        public Configuration Configuration { get; }
        public HttpConfiguration HttpConfiguration { get; }
        public virtual ActionMethod ActionMethod { get; }
        public virtual RouteDescriptor RouteDescriptor { get; }
    }
}