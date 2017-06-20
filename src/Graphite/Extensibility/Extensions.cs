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

        public static TPlugin ThatAppliesToOrDefault<TPlugin, TConfigContext>(
            this PluginDefinitions<TPlugin, TConfigContext> definitions,
            IEnumerable<TPlugin> plugins, TConfigContext configContext)
            where TPlugin : class, IConditional
        {
            return definitions.ThatApplyTo(plugins, configContext,
                    x => x.Applies()).FirstOrDefault() ??
                definitions.GetInstanceDefinitions(plugins)
                    .FirstOrDefault(x => x.IsDefault)?.Instance;
        }

        public static TPlugin ThatAppliesToOrDefault<TPlugin, TConfigContext, TPluginContext>(
            this PluginDefinitions<TPlugin, TConfigContext> definitions,
            IEnumerable<TPlugin> plugins, TConfigContext configContext, TPluginContext pluginContext)
            where TPlugin : class, IConditional<TPluginContext>
        {
            return definitions.ThatApplyTo(plugins, configContext, 
                x => x.AppliesTo(pluginContext)).FirstOrDefault() ??
                   definitions.GetInstanceDefinitions(plugins)
                    .FirstOrDefault(x => x.IsDefault)?.Instance;
        }

        private static IEnumerable<TPlugin> ThatApplyTo<TPlugin, TConfigContext>(
            this PluginDefinitions<TPlugin, TConfigContext> definitions,
            IEnumerable<TPlugin> plugins, TConfigContext configContext, Func<TPlugin, bool> predicate)
        {
            return definitions.GetInstanceDefinitions(plugins)
                .Where(x => (x.AppliesTo?.Invoke(configContext) ?? true) && predicate(x.Instance))
                .Select(x => x.Instance).ToList();
        }

        private static List<InstanceContext<TPlugin, TConfigContext>> GetInstanceDefinitions<TPlugin, TConfigContext>(
            this PluginDefinitions<TPlugin, TConfigContext> definitions,
            IEnumerable<TPlugin> plugins)
        {
            return plugins
                .Where(x => x != null)
                .GroupJoin(definitions, d => d.GetType(), p => p.Type, (p, d) => new
                {
                    Instance = p,
                    Order = definitions.Order(p),
                    Definition = d.FirstOrDefault()
                })
                .OrderBy(x => x.Order)
                .Select(x => new InstanceContext<TPlugin, TConfigContext>
                {
                    Instance = x.Instance,
                    AppliesTo = x.Definition?.AppliesTo,
                    IsDefault = definitions.IsDefault(x.Definition?.Type)
                }).ToList();
        }

        private class InstanceContext<TPlugin, TConfigContext>
        {
            public TPlugin Instance { get; set; }
            public Func<TConfigContext, bool> AppliesTo { get; set; }
            public bool IsDefault { get; set; }
        }
    }
}
