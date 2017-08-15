using System.Collections.Generic;
using System.Web.Http;
using Graphite.Extensibility;
using Graphite.Linq;
using Graphite.Reflection;

namespace Graphite.Actions
{
    public static class Extensions
    {
        public static IEnumerable<IActionDecorator> ThatApplyTo(
            this IEnumerable<IActionDecorator> actionDecorators,
            ActionDescriptor actionDescriptor, Configuration configuration,
            HttpConfiguration httpConfiguration)
        {
            return configuration.ActionDecorators.ThatAppliesTo(actionDecorators,
                new ActionConfigurationContext(configuration, httpConfiguration, 
                    actionDescriptor.Action, actionDescriptor.Route), 
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
