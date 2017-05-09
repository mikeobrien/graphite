using System.Collections.Generic;
using Graphite.Actions;
using Graphite.Extensibility;

namespace Graphite.Routing
{
    public class RouteContext
    {
        public RouteContext(ActionMethod actionMethod)
        {
            ActionMethod = actionMethod;
        }
        
        public virtual ActionMethod ActionMethod { get; }
    }

    public interface IRouteConvention : IConditional<RouteContext>
    {
        List<RouteDescriptor> GetRouteDescriptors(RouteContext context);
    }
}