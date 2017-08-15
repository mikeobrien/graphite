using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
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
            Configuration configuration, HttpConfiguration httpConfiguration)
        {
            var context = new ActionConfigurationContext(configuration, 
                httpConfiguration, actionMethod, routeDescriptor);
            return behaviors.ThatApplyTo(context)
                .Select(x => x.Type).ToList();
        }
    }
}
