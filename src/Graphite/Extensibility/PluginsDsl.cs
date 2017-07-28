namespace Graphite.Extensibility
{
    public class PluginsDsl<TPlugin>
    {
        protected readonly Plugins<TPlugin> Plugins;

        public PluginsDsl(Plugins<TPlugin> plugins)
        {
            Plugins = plugins;
        }

        public PluginsDsl<TPlugin> Clear()
        {
            Plugins.Clear();
            return this;
        }

        public PluginsDsl<TPlugin> ClearExcept<TConcrete>() where TConcrete : TPlugin
        {
            Plugins.ClearExcept<TConcrete>();
            return this;
        }

        public PluginsDsl<TPlugin> DefaultIs<TConcrete>() where TConcrete : TPlugin
        {
            Plugins.DefaultIs(Plugins.FirstOfType<TConcrete>());
            return this;
        }

        public PluginsDsl<TPlugin> Remove<TConcrete>(
            bool typeOnly = false) where TConcrete : TPlugin
        {
            Plugins.RemoveAllOfType<TConcrete>();
            return this;
        }

        public ReplaceDsl<TReplace> Replace<TReplace>()
            where TReplace : TPlugin
        {
            return new ReplaceDsl<TReplace>(this, Plugins);
        }

        public class ReplaceDsl<TReplace> where TReplace : TPlugin
        {
            private readonly PluginsDsl<TPlugin> _dsl;
            private readonly Plugins<TPlugin> _plugins;

            public ReplaceDsl(PluginsDsl<TPlugin> dsl,
                Plugins<TPlugin> plugins)
            {
                _dsl = dsl;
                _plugins = plugins;
            }

            public PluginsDsl<TPlugin> With<TReplacement>(bool @default = false)
                where TReplacement : TPlugin
            {
                return With(Plugin<TPlugin>
                    .Create<TReplacement>(_plugins.Singleton), @default);
            }

            public PluginsDsl<TPlugin> With<TReplacement>(
                TReplacement instance, bool dispose = false, bool @default = false)
                where TReplacement : TPlugin
            {
                return With(Plugin<TPlugin>
                    .Create(instance, dispose), @default);
            }

            private PluginsDsl<TPlugin> With(
                Plugin<TPlugin> plugin, bool @default = false)
            {
                _plugins.ReplaceAllOfTypeWith<TReplace>(plugin, @default);
                return _dsl;
            }
        }

        public AppendDsl Append<TAppend>(bool @default = false)
            where TAppend : TPlugin
        {
            return Append(Plugin<TPlugin>
                .Create<TAppend>(Plugins.Singleton), @default);
        }

        public AppendDsl Append<TAppend>(TAppend instance, 
            bool dispose = false, bool @default = false)
            where TAppend : TPlugin
        {
            return Append(Plugin<TPlugin>
                .Create(instance, dispose), @default);
        }

        private AppendDsl Append(Plugin<TPlugin> plugin, bool @default = false)
        {
            Plugins.Append(plugin, @default);
            return new AppendDsl(plugin, Plugins);
        }

        public class AppendDsl : PluginsDsl<TPlugin>
        {
            private readonly Plugin<TPlugin> _plugin;

            public AppendDsl(Plugin<TPlugin> plugin,
                Plugins<TPlugin> plugins) : base(plugins)
            {
                _plugin = plugin;
            }

            public PluginsDsl<TPlugin> AfterOrPrepend<TFind>()
                where TFind : TPlugin
            {
                Plugins.AppendAfterOrPrepend<TFind>(_plugin);
                return this;
            }

            public PluginsDsl<TPlugin> AfterOrAppend<TFind>()
                where TFind : TPlugin
            {
                Plugins.AppendAfterOrAppend<TFind>(_plugin);
                return this;
            }
        }

        public PrependDsl Prepend<TPrepend>(bool @default = false) 
            where TPrepend : TPlugin
        {
            return Prepend(Plugin<TPlugin>
                .Create<TPrepend>(Plugins.Singleton), @default);
        }

        public PrependDsl Prepend<TPrepend>(
            TPrepend instance, bool dispose = false, bool @default = false) 
            where TPrepend : TPlugin
        {
            return Prepend(Plugin<TPlugin>
                .Create(instance, dispose), @default);
        }

        private PrependDsl Prepend(
            Plugin<TPlugin> plugin, bool @default = false)
        {
            Plugins.Prepend(plugin, @default);
            return new PrependDsl(plugin, Plugins);
        }

        public class PrependDsl : PluginsDsl<TPlugin>
        {
            private readonly Plugin<TPlugin> _plugin;

            public PrependDsl(Plugin<TPlugin> plugin,
                Plugins<TPlugin> plugins) : base(plugins)
            {
                _plugin = plugin;
            }

            public PluginsDsl<TPlugin> BeforeOrPrepend<TFind>()
                where TFind : TPlugin
            {
                Plugins.PrependBeforeOrPrepend<TFind>(_plugin);
                return this;
            }

            public PluginsDsl<TPlugin> BeforeOrAppend<TFind>()
                where TFind : TPlugin
            {
                Plugins.PrependBeforeOrAppend<TFind>(_plugin);
                return this;
            }
        }
    }
}
