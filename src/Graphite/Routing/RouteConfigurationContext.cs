using System.Web.Http;
using Graphite.Actions;

namespace Graphite.Routing
{
    public class RouteConfigurationContext
    {
        public RouteConfigurationContext(
            ConfigurationContext configurationContext, 
            ActionMethod actionMethod)
        {
            Configuration = configurationContext.Configuration;
            HttpConfiguration = configurationContext.HttpConfiguration;
            ActionMethod = actionMethod;
        }

        public Configuration Configuration { get; }
        public HttpConfiguration HttpConfiguration { get; }
        public ActionMethod ActionMethod { get; }
    }
}