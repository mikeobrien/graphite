using System;
using Graphite.Actions;
using Graphite.Extensibility;
using Graphite.Reflection;

namespace Graphite.Setup
{
    public partial class ConfigurationDsl
    {
        /// <summary>
        /// Configures action method sources.
        /// </summary>
        public ConfigurationDsl ConfigureActionMethodSources(
            Action<PluginsDsl<IActionMethodSource>> configure)
        {
            _configuration.ActionMethodSources.Configure(configure);
            return this;
        }

        /// <summary>
        /// Configures action sources.
        /// </summary>
        public ConfigurationDsl ConfigureActionSources(
            Action<PluginsDsl<IActionSource>> configure)
        {
            _configuration.ActionSources.Configure(configure);
            return this;
        }

        /// <summary>
        /// Specifies the handler filter.
        /// </summary>
        public ConfigurationDsl FilterHandlersBy(Func<Configuration, TypeDescriptor, bool> filter)
        {
            _configuration.HandlerFilter = filter;
            return this;
        }

        /// <summary>
        /// Only includes handlers under the namespace of the specified type.
        /// </summary>
        public ConfigurationDsl OnlyIncludeHandlersUnder<T>()
        {
            _configuration.HandlerFilter = (c, t) =>  t.Type.IsUnderNamespace<T>();
            return this;
        }

        /// <summary>
        /// Specifies the action filter.
        /// </summary>
        public ConfigurationDsl FilterActionsBy(Func<Configuration, MethodDescriptor, bool> filter)
        {
            _configuration.ActionFilter = filter;
            return this;
        }

        /// <summary>
        /// Configures action decorators.
        /// </summary>
        public ConfigurationDsl ConfigureActionDecorators(Action<ConditionalPluginsDsl
            <IActionDecorator, ActionConfigurationContext>> configure)
        {
            _configuration.ActionDecorators.Configure(configure);
            return this;
        }

        /// <summary>
        /// Specifies the action invoker to use.
        /// </summary>
        public ConfigurationDsl WithActionInvoker<T>() where T : IActionInvoker
        {
            _configuration.ActionInvoker.Set<T>();
            return this;
        }

        /// <summary>
        /// Specifies the action invoker to use.
        /// </summary>
        public ConfigurationDsl WithActionInvoker<T>(T instance) where T : IActionInvoker
        {
            _configuration.ActionInvoker.Set(instance);
            return this;
        }
    }
}
