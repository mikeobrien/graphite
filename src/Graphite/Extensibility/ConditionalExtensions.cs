using System;
using System.Collections.Generic;
using System.Linq;
using Graphite.Extensions;

namespace Graphite.Extensibility
{
    public static class ConditionalExtensions
    {
        public static IEnumerable<TPlugin> ThatApplyToOrDefault<TPlugin, TContext>(
            this Plugins<TPlugin> plugins, IEnumerable<TPlugin> instances, TContext context)
            where TPlugin : IConditional<TContext>
        {
            return plugins.ThatApplyOrDefault(instances, plugins.ThatApplyTo(instances, context));
        }

        public static IEnumerable<TPlugin> ThatApplyOrDefault<TPlugin>(
            this Plugins<TPlugin> plugins, IEnumerable<TPlugin> instances)
            where TPlugin : IConditional
        {
            return plugins.ThatApplyOrDefault(instances, plugins.ThatApply(instances));
        }

        public static IEnumerable<TPlugin> ThatApply<TPlugin>(
            this Plugins<TPlugin> plugins,
            IEnumerable<TPlugin> instances)
            where TPlugin : IConditional
        {
            return plugins.ThatApply(instances, x => x.Applies());
        }

        public static IEnumerable<TPlugin> ThatApplyTo<TPlugin, TContext>(
            this Plugins<TPlugin> plugins,
            IEnumerable<TPlugin> instances, TContext context)
            where TPlugin : IConditional<TContext>
        {
            return plugins.ThatApply(instances, x => x.AppliesTo(context));
        }

        private static IEnumerable<TPlugin> ThatApplyOrDefault<TPlugin>(
            this Plugins<TPlugin> plugins, IEnumerable<TPlugin> instances,
            IEnumerable<TPlugin> thatApply)
        {
            return thatApply.AnyOrDefault(() => plugins.GetDefaultInstance(instances));
        }

        private static IEnumerable<TPlugin> ThatApply<TPlugin>(
            this Plugins<TPlugin> plugins, IEnumerable<TPlugin> instances, 
            Func<TPlugin, bool> predicate)
        {
            return plugins.PluginsFor(instances)
                .Where(x => predicate.Invoke(x.Instance))
                .Select(x => x.Instance).ToList();
        }





        public static IEnumerable<TPlugin> ThatApply<TPlugin, TPluginContext>(
            this ConditionalPlugins<TPlugin, TPluginContext> plugins,
            IEnumerable<TPlugin> instances, TPluginContext pluginContext)
            where TPlugin : IConditional
        {
            return plugins.ThatApply(instances, pluginContext, x => x.Applies());
        }

        public static TPlugin FirstThatAppliesOrDefault<TPlugin, TPluginContext>(
            this ConditionalPlugins<TPlugin, TPluginContext> plugins,
            IEnumerable<TPlugin> instances, TPluginContext pluginContext)
            where TPlugin : class, IConditional
        {
            return plugins.ThatAppliesOrDefault(instances, pluginContext).FirstOrDefault();
        }

        public static IEnumerable<TPlugin> ThatAppliesOrDefault<TPlugin, TPluginContext>(
            this ConditionalPlugins<TPlugin, TPluginContext> plugins,
            IEnumerable<TPlugin> instances, TPluginContext pluginContext)
            where TPlugin : class, IConditional
        {
            return plugins.ThatApplyOrDefault(instances, pluginContext, x => x.Applies());
        }

        public static IEnumerable<TPlugin> ThatAppliesTo<TPlugin, TPluginContext, TInstanceContext>(
            this ConditionalPlugins<TPlugin, TPluginContext> plugins,
            IEnumerable<TPlugin> instances, TPluginContext pluginContext, TInstanceContext instanceContext)
            where TPlugin : IConditional<TInstanceContext>
        {
            return plugins.ThatApply(instances, pluginContext, x => x.AppliesTo(instanceContext));
        }

        public static TPlugin FirstThatAppliesToOrDefault<TPlugin, TPluginContext, TInstanceContext>(
            this ConditionalPlugins<TPlugin, TPluginContext> plugins,
            IEnumerable<TPlugin> instances, TPluginContext pluginContext, TInstanceContext instanceContext)
            where TPlugin : class, IConditional<TInstanceContext>
        {
            return plugins.ThatAppliesToOrDefault(instances, 
                pluginContext, instanceContext).FirstOrDefault();
        }

        public static IEnumerable<TPlugin> ThatAppliesToOrDefault<TPlugin, TPluginContext, TInstanceContext>(
            this ConditionalPlugins<TPlugin, TPluginContext> plugins,
            IEnumerable<TPlugin> instances, TPluginContext pluginContext, TInstanceContext instanceContext)
            where TPlugin : class, IConditional<TInstanceContext>
        {
            return plugins.ThatApplyOrDefault(instances, pluginContext,
                x => x.AppliesTo(instanceContext));
        }

        private static IEnumerable<TPlugin> ThatApplyOrDefault<TPlugin, TPluginContext>(
            this ConditionalPlugins<TPlugin, TPluginContext> plugins,
            IEnumerable<TPlugin> instances, TPluginContext pluginContext, Func<TPlugin, bool> predicate)
        {
            return plugins.ThatApply(instances, pluginContext, predicate).ToList()
                .AnyOrDefault(() => plugins.GetDefaultInstance(instances));
        }

        private static IEnumerable<TPlugin> ThatApply<TPlugin, TPluginContext>(
            this ConditionalPlugins<TPlugin, TPluginContext> plugins,
            IEnumerable<TPlugin> instances, TPluginContext pluginContext, Func<TPlugin, bool> predicate)
        {
            return plugins.ThatApplyTo(instances, pluginContext)
                .Where(x => predicate.Invoke(x.Instance))
                .Select(x => x.Instance).ToList();
        }
    }
}
