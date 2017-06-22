using System;
using System.Collections.Generic;
using System.Linq;
using Graphite.DependencyInjection;
using Graphite.Extensions;

namespace Graphite.Extensibility
{
    public static class Extensions
    {
        public static IContainer RegisterPlugins<TPlugin, TContext>(this IContainer container,
            PluginDefinitions<TPlugin, TContext> pluginDefinitions) 
            where TPlugin : class
        {
            pluginDefinitions.ForEach(x => container.RegisterPlugin(x));
            return container;
        }

        public static IContainer RegisterPlugin<TPlugin>(this IContainer container,
            PluginDefinition<TPlugin> pluginDefinition) where TPlugin : class
        {
            if (pluginDefinition.HasInstance) container.Register(
                typeof(TPlugin), pluginDefinition.Instance, pluginDefinition.Dispose);
            else container.Register<TPlugin>(pluginDefinition.Type, pluginDefinition.Singleton);
            return container;
        }

        public static Registry RegisterPlugins<TPlugin, TContext>(this Registry registry,
            PluginDefinitions<TPlugin, TContext> pluginDefinitions)
            where TPlugin : class
        {
            pluginDefinitions.ForEach(x => registry.RegisterPlugin(x));
            return registry;
        }

        public static Registry RegisterPlugin<TPlugin>(this Registry registry,
            PluginDefinition<TPlugin> pluginDefinition) where TPlugin : class
        {
            if (pluginDefinition.HasInstance) registry.Register(
                typeof(TPlugin), pluginDefinition.Instance, pluginDefinition.Dispose);
            else registry.Register<TPlugin>(pluginDefinition.Type, pluginDefinition.Singleton);
            return registry;
        }

        public static TPlugin GetInstance<TPlugin>(this PluginDefinition<TPlugin> pluginDefinition,
            IContainer container) where TPlugin : class
        {
            return (TPlugin)(pluginDefinition.HasInstance ? pluginDefinition.Instance : 
                container.GetInstance(pluginDefinition.Type));
        }

        public static IEnumerable<TPlugin> ThatApplies<TPlugin, TConfigContext>(
            this PluginDefinitions<TPlugin, TConfigContext> definitions,
            IEnumerable<TPlugin> plugins, TConfigContext configContext)
            where TPlugin : IConditional
        {
            return definitions.ThatApplies(plugins, configContext, x => x.Applies());
        }

        public static IEnumerable<TPlugin> ThatAppliesTo<TPlugin, TConfigContext, TPluginContext>(
            this PluginDefinitions<TPlugin, TConfigContext> definitions,
            IEnumerable<TPlugin> plugins, TConfigContext configContext, TPluginContext pluginContext)
            where TPlugin : IConditional<TPluginContext>
        {
            return definitions.ThatApplies(plugins, configContext, x => x.AppliesTo(pluginContext));
        }

        public static TPlugin FirstThatAppliesOrDefault<TPlugin, TConfigContext>(
            this PluginDefinitions<TPlugin, TConfigContext> definitions,
            IEnumerable<TPlugin> plugins, TConfigContext configContext)
            where TPlugin : class, IConditional
        {
            return definitions.ThatAppliesOrDefault(plugins, configContext).FirstOrDefault();
        }

        public static IEnumerable<TPlugin> ThatAppliesOrDefault<TPlugin, TConfigContext>(
            this PluginDefinitions<TPlugin, TConfigContext> definitions,
            IEnumerable<TPlugin> plugins, TConfigContext configContext)
            where TPlugin : class, IConditional
        {
            var thatApply = definitions.ThatApplies(plugins, configContext,
                x => x.Applies()).ToList();
            return thatApply.Any() ? thatApply : definitions.GetDefault(plugins);
        }

        public static TPlugin FirstThatAppliesToOrDefault<TPlugin, TConfigContext, TPluginContext>(
            this PluginDefinitions<TPlugin, TConfigContext> definitions,
            IEnumerable<TPlugin> plugins, TConfigContext configContext, TPluginContext pluginContext)
            where TPlugin : class, IConditional<TPluginContext>
        {
            return definitions.ThatAppliesToOrDefault(plugins, 
                configContext, pluginContext).FirstOrDefault();
        }

        public static IEnumerable<TPlugin> ThatAppliesToOrDefault<TPlugin, TConfigContext, TPluginContext>(
            this PluginDefinitions<TPlugin, TConfigContext> definitions,
            IEnumerable<TPlugin> plugins, TConfigContext configContext, TPluginContext pluginContext)
            where TPlugin : class, IConditional<TPluginContext>
        {
            var thatApply = definitions.ThatApplies(plugins, configContext,
                x => x.AppliesTo(pluginContext)).ToList();
            return thatApply.Any() ? thatApply : definitions.GetDefault(plugins);
        }

        private static IEnumerable<TPlugin> ThatApplies<TPlugin, TConfigContext>(
            this PluginDefinitions<TPlugin, TConfigContext> definitions,
            IEnumerable<TPlugin> plugins, TConfigContext configContext, Func<TPlugin, bool> predicate)
        {
            return definitions.GetInstanceDefinitions(plugins)
                .Where(x => (x.AppliesTo?.Invoke(configContext) ?? true) && 
                    (predicate?.Invoke(x.Instance) ?? true))
                .Select(x => x.Instance).ToList();
        }

        private static IEnumerable<TPlugin> GetDefault<TPlugin, TConfigContext>(
            this PluginDefinitions<TPlugin, TConfigContext> definitions,
            IEnumerable<TPlugin> plugins)
        {
            return definitions.GetInstanceDefinitions(plugins)
                .Where(x => x.IsDefault).Select(x => x.Instance); ;
        }

        private static List<InstanceContext<TPlugin, TConfigContext>> GetInstanceDefinitions<TPlugin, TConfigContext>(
            this PluginDefinitions<TPlugin, TConfigContext> definitions,
            IEnumerable<TPlugin> plugins)
        {
            return plugins
                .Where(x => x != null)
                .Select(x => new
                {
                    Instance = x,
                    Definition = definitions.GetFirst(x)
                })
                .OrderBy(x => definitions.Order(x.Definition))
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
