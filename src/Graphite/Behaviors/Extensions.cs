using System;
using System.Collections.Generic;
using System.Linq;
using Graphite.Actions;
using Graphite.Extensibility;
using Graphite.Routing;

namespace Graphite.Behaviors
{
    public static class Extensions
    {
        public static List<Type> ThatApplyTo(this PluginDefinitions
            <IBehavior, BehaviorContext> behaviors, ActionSourceContext context,
            ActionMethod actionMethod, RouteDescriptor routeDescriptor)
        {
            return behaviors.ThatApplyTo(new BehaviorContext(context.Configuration, context
                .HttpConfiguration, actionMethod, routeDescriptor))
                .Select(x => x.Type).ToList();
        }
    }
}
