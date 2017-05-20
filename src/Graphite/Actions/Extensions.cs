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
        public static IEnumerable<IActionMethodSource> ThatApplyTo(
            this IEnumerable<IActionMethodSource> methodSources,
            ActionSourceContext actionSourceContext, Configuration configuration)
        {
            return configuration.ActionMethodSources.ThatApplyTo(methodSources, 
                new ActionMethodSourceContext(configuration, actionSourceContext.HttpConfiguration));
        }

        public static IEnumerable<ActionMethod> GetActionMethods(this
            IActionMethodSource actionMethodSource, ActionSourceContext context)
        {
            return actionMethodSource.GetActionMethods(new 
                ActionMethodSourceContext(context.Configuration, context.HttpConfiguration));
        }

        public static List<ActionDescriptor> GetActions(this IActionSource actionSource,
            Configuration configuration, HttpConfiguration httpConfiguration)
        {
            return actionSource.GetActions(new ActionSourceContext(configuration, httpConfiguration));
        }

        public static IEnumerable<IActionSource> ThatApplyTo(
            this IEnumerable<IActionSource> actionSources,
            HttpConfiguration httpConfiguration, Configuration configuration)
        {
            return configuration.ActionSources.ThatApplyTo(actionSources, 
                new ActionSourceContext(configuration, httpConfiguration));
        }

        public static IEnumerable<IActionDecorator> ThatApplyTo(
            this IEnumerable<IActionDecorator> actionDecorators,
            ActionDescriptor actionDescriptor, HttpConfiguration httpConfiguration, 
            Configuration configuration)
        {
            return configuration.ActionDecorators.ThatApplyTo(actionDecorators,
                new ActionDecoratorContext(configuration, httpConfiguration, actionDescriptor));
        }

        public static void Decorate(this IActionDecorator actionSource,
            ActionDescriptor actionDescriptor, HttpConfiguration httpConfiguration,
            Configuration configuration)
        {
            actionSource.Decorate(new ActionDecoratorContext(configuration, 
                httpConfiguration, actionDescriptor));
        }
    }
}
