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
        public static List<Type> ThatApplyTo(this PluginDefinitions
            <IBehavior, ActionConfigurationContext> behaviors, 
            ActionMethod actionMethod, RouteDescriptor routeDescriptor, 
            ConfigurationContext configurationContext)
        {
            return behaviors.ThatApplyTo(new ActionConfigurationContext(
                configurationContext, actionMethod, routeDescriptor))
                .Select(x => x.Type).ToList();
        }
    }
}
