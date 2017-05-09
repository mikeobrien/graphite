using System;
using System.Collections.Generic;
using System.Linq;
using Graphite.DependencyInjection;
using Graphite.Extensions;

namespace Graphite.Extensibility
{
    public static class Extensions
    {
        public static void RegisterPlugins<TPlugin, TContext>(this IContainer container,
            PluginDefinitions<TPlugin, TContext> pluginDefinitions) 
            where TPlugin : class
        {
            pluginDefinitions.ForEach(container.RegisterPlugin);
        }

        public static void RegisterPlugin<TPlugin>(this IContainer container,
            PluginDefinition<TPlugin> pluginDefinition) where TPlugin : class
        {
            if (pluginDefinition.HasInstance) container.Register(
                typeof(TPlugin), pluginDefinition.Instance, pluginDefinition.Dispose);
            else container.Register<TPlugin>(pluginDefinition.Type, pluginDefinition.Singleton);
        }

        public static TPlugin GetInstance<TPlugin>(this PluginDefinition<TPlugin> pluginDefinition,
            IContainer container) where TPlugin : class
        {
            return (TPlugin)(pluginDefinition.HasInstance ? pluginDefinition.Instance : 
                container.GetInstance(pluginDefinition.Type));
        }

        public static IEnumerable<TPlugin> ThatApplyTo<TPlugin, TConfigContext>(
            this PluginDefinitions<TPlugin, TConfigContext> definitions,
            IEnumerable<TPlugin> plugins, TConfigContext configContext)
            where TPlugin : IConditional
        {
            return definitions.ThatApplyTo(plugins, configContext, x => x.Applies());
        }

        public static IEnumerable<TPlugin> ThatApplyTo<TPlugin, TConfigContext, TPluginContext>(
            this PluginDefinitions<TPlugin, TConfigContext> definitions,
            IEnumerable<TPlugin> plugins, TConfigContext configContext, TPluginContext pluginContext)
            where TPlugin : IConditional<TPluginContext>
        {
            return definitions.ThatApplyTo(plugins, configContext, x => x.AppliesTo(pluginContext));
        }

        private static IEnumerable<TPlugin> ThatApplyTo<TPlugin, TConfigContext>(
            this PluginDefinitions<TPlugin, TConfigContext> definitions,
            IEnumerable<TPlugin> plugins, TConfigContext configContext, Func<TPlugin, bool> predicate)
        {
            return plugins
                .Where(x => x != null)
                .GroupJoin(definitions, d => d.GetType(), p => p.Type, (p, d) => new
                {
                    Instance = p,
                    Order = definitions.Order(p),
                    d.FirstOrDefault()?.AppliesTo
                })
                .Where(x => (x.AppliesTo?.Invoke(configContext) ?? true) && predicate(x.Instance))
                .OrderBy(x => x.Order)
                .Select(x => x.Instance).ToList();
        }
    }
}
