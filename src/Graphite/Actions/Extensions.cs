using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Graphite.Binding;
using Graphite.Extensibility;
using Graphite.Routing;

namespace Graphite.Actions
{
    public static class Extensions
    {
        public static List<ActionDescriptor> GetActions(this IActionSource actionSource,
            Configuration configuration, HttpConfiguration httpConfiguration)
        {
            return actionSource.GetActions(new ActionSourceContext(configuration, httpConfiguration));
        }

        public static List<Type> ThatApplyTo(this PluginDefinitions
            <IBehavior, BehaviorContext> behaviors, ActionSourceContext context,
            ActionMethod actionMethod, RouteDescriptor routeDescriptor)
        {
            return behaviors.ThatApplyTo(new BehaviorContext(context.Configuration, context
                .HttpConfiguration, actionMethod, routeDescriptor))
                .Select(x => x.Type).ToList();
        }

        public static IEnumerable<IActionMethodSource> ThatApplyTo(
            this IEnumerable<IActionMethodSource> methodSources,
            ActionSourceContext actionSourceContext, Configuration configuration)
        {
            return configuration.ActionMethodSources.ThatApplyTo(methodSources, 
                new ActionMethodSourceContext(configuration, actionSourceContext.HttpConfiguration));
        }

        public static IEnumerable<IActionSource> ThatApplyTo(
            this IEnumerable<IActionSource> actionSources,
            HttpConfiguration httpConfiguration, Configuration configuration)
        {
            return configuration.ActionSources.ThatApplyTo(actionSources, 
                new ActionSourceContext(configuration, httpConfiguration));
        }

        public static IEnumerable<ActionMethod> GetActionMethods(this
            IActionMethodSource actionMethodSource, ActionSourceContext context)
        {
            return actionMethodSource.GetActionMethods(new 
                ActionMethodSourceContext(context.Configuration, context.HttpConfiguration));
        }

        public static Task Bind(this IRequestBinder binder, 
            Configuration configuration, RequestContext context, 
            object[] actionArguments)
        {
            return binder.Bind(new RequestBinderContext(configuration, context, actionArguments));
        }
    }
}
