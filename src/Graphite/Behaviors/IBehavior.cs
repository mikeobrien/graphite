using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Graphite.Actions;
using Graphite.Routing;

namespace Graphite.Behaviors
{
    public class BehaviorContext
    {
        public BehaviorContext(Configuration configuration,
            HttpConfiguration httpConfiguration, ActionMethod actionMethod, 
            RouteDescriptor routeDescriptor)
        {
            Configuration = configuration;
            HttpConfiguration = httpConfiguration;
            ActionMethod = actionMethod;
            RouteDescriptor = routeDescriptor;
        }

        public virtual Configuration Configuration { get; }
        public virtual HttpConfiguration HttpConfiguration { get; }
        public virtual ActionMethod ActionMethod { get; }
        public virtual RouteDescriptor RouteDescriptor { get; }
    }

    public interface IBehavior
    {
        bool ShouldRun();
        Task<HttpResponseMessage> Invoke();
    }
}
