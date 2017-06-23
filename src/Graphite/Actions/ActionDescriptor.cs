using System.Collections.Generic;
using Graphite.DependencyInjection;
using Graphite.Routing;
using Graphite.Extensions;
using Graphite.Reflection;

namespace Graphite.Actions
{
    public class ActionDescriptor
    {
        public ActionDescriptor(ActionMethod action, 
            RouteDescriptor route, IEnumerable<TypeDescriptor> behaviors = null)
        {
            Action = action;
            Route = route;
            Behaviors = behaviors ?? new List<TypeDescriptor>();
            Registry = new Registry();
        }

        public virtual ActionMethod Action { get; }
        public virtual RouteDescriptor Route { get; }
        public virtual IEnumerable<TypeDescriptor> Behaviors { get; }
        public virtual Registry Registry { get; }

        public override int GetHashCode()
        {
            return this.GetHashCode(Route?.Id);
        }

        public override bool Equals(object obj)
        {
            return GetHashCode() == obj?.GetHashCode();
        }

        public override string ToString()
        {
            return Route.Id;
        }
    }
}
