using System;
using System.Collections.Generic;
using System.Linq;
using Graphite.Extensibility;
using Graphite.Actions;
using Graphite.Routing;

namespace Graphite.Behaviors
{
    public static class Extensions
    {
        public static List<Type> ThatApplyTo(this ConditionalPlugins
                <IBehavior, ActionConfigurationContext> behaviors,
            ActionMethod actionMethod, RouteDescriptor routeDescriptor,
            ConfigurationContext configurationContext)
        {
            var context = new ActionConfigurationContext(
                configurationContext, actionMethod, routeDescriptor);
            return behaviors.ThatApplyTo(context)
                .Select(x => x.Type).ToList();
        }
    }
}
