using System;
using System.Collections.Generic;
using System.Linq;

namespace Graphite.Extensibility
{
    public class ConditionalPlugins<TPlugin, TContext> : 
        PluginsBase<TPlugin, ConditionalPlugin<TPlugin, TContext>>
    {
        public ConditionalPlugins(bool singleton) : base(singleton) { }

        public ConditionalPlugins(
            IEnumerable<ConditionalPlugin<TPlugin, TContext>> plugins,
            bool singleton, ConditionalPlugin<TPlugin, TContext> @default) :
            base(plugins, singleton, @default) { }

        public ConditionalPlugins<TPlugin, TContext> Configure(
            Action<ConditionalPluginsDsl<TPlugin, TContext>> configure)
        {
            configure(new ConditionalPluginsDsl<TPlugin, TContext>(this));
            return this;
        }

        public IEnumerable<ConditionalPlugin<TPlugin, TContext>> 
            ThatApplyTo(TContext context)
        {
            return this.Where(x => x.AppliesTo?.Invoke(context) ?? true);
        }

        public IEnumerable<PluginInstance> ThatApplyTo(
            IEnumerable<TPlugin> plugins, TContext context)
        {
            return PluginsFor(plugins)
                .Where(x => x.Plugin.AppliesTo?.Invoke(context) ?? true);
        }

        public Plugins<TPlugin> CloneAllThatApplyTo(TContext context)
        {
            return new Plugins<TPlugin>(Plugins
                    .Where(x => (x.AppliesTo?.Invoke(context) ?? true) || IsDefault(x))
                    .Cast<Plugin<TPlugin>>()
                    .Select(x => x.Clone()), 
                Singleton, Default?.Clone());
        }
    }
}