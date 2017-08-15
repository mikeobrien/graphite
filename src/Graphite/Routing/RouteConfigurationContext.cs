using System.Web.Http;
using Graphite.Actions;

namespace Graphite.Routing
{
    public class RouteConfigurationContext
    {
        public RouteConfigurationContext(Configuration configuration,
            HttpConfiguration httpConfiguration,
            ActionMethod actionMethod)
        {
            Configuration = configuration;
            HttpConfiguration = httpConfiguration;
            ActionMethod = actionMethod;
        }

        public Configuration Configuration { get; }
        public HttpConfiguration HttpConfiguration { get; }
        public ActionMethod ActionMethod { get; }
    }
}