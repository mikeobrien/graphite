using System.Collections.Generic;
using Graphite.Extensibility;
using Graphite.Extensions;
using Graphite.Reflection;

namespace Graphite.Actions
{
    public static class Extensions
    {
        public static IEnumerable<IActionMethodSource> ThatApplyTo(
            this IEnumerable<IActionMethodSource> methodSources, 
            ConfigurationContext configurationContext)
        {
            return configurationContext.Configuration.ActionMethodSources
                .ThatApplyTo(methodSources, configurationContext);
        }

        public static IEnumerable<IActionSource> ThatApplyTo(
            this IEnumerable<IActionSource> actionSources,
            ConfigurationContext configurationContext)
        {
            return configurationContext.Configuration.ActionSources
                .ThatApplyTo(actionSources, configurationContext);
        }

        public static IEnumerable<IActionDecorator> ThatApplyTo(
            this IEnumerable<IActionDecorator> actionDecorators,
            ActionDescriptor actionDescriptor, ConfigurationContext configurationContext)
        {
            return configurationContext.Configuration.ActionDecorators.ThatApplyTo(actionDecorators,
                new ActionConfigurationContext(configurationContext, actionDescriptor), 
                new ActionDecoratorContext(actionDescriptor));
        }

        public static void Decorate(this IEnumerable<IActionDecorator> 
            actionSources, ActionDescriptor actionDescriptor)
        {
            var context = new ActionDecoratorContext(actionDescriptor);
            actionSources.ForEach(x => x.Decorate(context));
        }

        public static bool IsUnderNamespace<T>(this ActionMethod actionMethod, string relativeNamespace = null)
        {
            return actionMethod.HandlerTypeDescriptor.Type.IsUnderNamespace<T>(relativeNamespace);
        }
    }
}
