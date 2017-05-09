using System;
using Graphite.Routing;
using Graphite.Extensions;
using Graphite.Reflection;

namespace Graphite.Actions
{
    public class ActionDescriptor
    {
        public ActionDescriptor(ActionMethod action, 
            RouteDescriptor route, TypeDescriptor[] behaviors = null)
        {
            Action = action;
            Route = route;
            Behaviors = behaviors ?? new TypeDescriptor[] {};
        }

        public virtual ActionMethod Action { get; }
        public virtual RouteDescriptor Route { get; }
        public virtual TypeDescriptor[] Behaviors { get; }

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
