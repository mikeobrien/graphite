using System;
using Graphite.Actions;
using Graphite.Behaviors;
using Graphite.Extensibility;

namespace Graphite.Setup
{
    public partial class ConfigurationDsl
    {
        /// <summary>
        /// Configures behaviors.
        /// </summary>
        public ConfigurationDsl ConfigureBehaviors(Action<ConditionalPluginsDsl
            <IBehavior, ActionConfigurationContext>> configure)
        {
            _configuration.Behaviors.Configure(configure);
            return this;
        }

        /// <summary>
        /// Specifies the behavior chain invoker to use.
        /// </summary>
        public ConfigurationDsl WithBehaviorChainInvoker<T>() where T : IBehaviorChainInvoker
        {
            _configuration.BehaviorChainInvoker.Set<T>();
            return this;
        }

        /// <summary>
        /// Specifies the behavior chain invoker to use.
        /// </summary>
        public ConfigurationDsl WithBehaviorChainInvoker<T>(T instance) where T : IBehaviorChainInvoker
        {
            _configuration.BehaviorChainInvoker.Set(instance);
            return this;
        }

        /// <summary>
        /// Specifies the behavior chain to use.
        /// </summary>
        public ConfigurationDsl WithBehaviorChain<T>() where T : IBehaviorChain
        {
            _configuration.BehaviorChain = typeof(T);
            return this;
        }

        /// <summary>
        /// Specifies the last behavior in the chain.
        /// </summary>
        public ConfigurationDsl WithDefaultBehavior<T>() where T : IBehavior
        {
            _configuration.DefaultBehavior = typeof(T);
            return this;
        }
    }
}
