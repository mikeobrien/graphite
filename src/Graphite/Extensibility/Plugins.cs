using System;
using System.Collections.Generic;

namespace Graphite.Extensibility
{
    public class Plugins<TPlugin> : PluginsBase<TPlugin, Plugin<TPlugin>>
    {
        public Plugins(bool singleton) : base(singleton) { }

        public Plugins(
            IEnumerable<Plugin<TPlugin>> plugins, 
            bool singleton, Plugin<TPlugin> @default) : 
            base(plugins, singleton, @default) { }

        public Plugins<TPlugin> Configure(
            Action<PluginsDsl<TPlugin>> configure)
        {
            configure(new PluginsDsl<TPlugin>(this));
            return this;
        }
    }
}