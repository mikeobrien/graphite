using System;
using System.Linq;

namespace Graphite.Extensibility
{
    public class ConditionalPluginsDsl<TPlugin, TContext>
    {
        protected readonly ConditionalPlugins<TPlugin, TContext> Plugins;
        protected readonly Func<TContext, bool> DefaultPredicate;

        public ConditionalPluginsDsl(
            ConditionalPlugins<TPlugin, TContext> plugins, 
            Func<TContext, bool> defaultPredicate = null)
        {
            Plugins = plugins;
            DefaultPredicate = defaultPredicate;
        }

        public ConditionalPluginsDsl<TPlugin, TContext> DefaultIs<TConcrete>() 
            where TConcrete : TPlugin
        {
            Plugins.DefaultIs(Plugins.FirstOfType<TConcrete>());
            return this;
        }

        public ConditionalPluginsDsl<TPlugin, TContext> Clear()
        {
            Plugins.Clear();
            return this;
        }

        public ConditionalPluginsDsl<TPlugin, TContext> ClearExcept<TConcrete>() 
            where TConcrete : TPlugin
        {
            Plugins.ClearExcept<TConcrete>();
            return this;
        }

        public ConditionalPluginsDsl<TPlugin, TContext> Remove<TConcrete>(
            bool typeOnly = false) where TConcrete : TPlugin
        {
            Plugins.RemoveAllOfType<TConcrete>();
            return this;
        }

        public ReplaceDsl<TReplace> Replace<TReplace>()
            where TReplace : TPlugin
        {
            return new ReplaceDsl<TReplace>(this, Plugins, DefaultPredicate);
        }

        public class ReplaceDsl<TReplace> where TReplace : TPlugin
        {
            private readonly ConditionalPluginsDsl<TPlugin, TContext> _dsl;
            private readonly ConditionalPlugins<TPlugin, TContext> _plugins;
            private readonly Func<TContext, bool> _defaultPredicate;

            public ReplaceDsl(ConditionalPluginsDsl<TPlugin, TContext> dsl,
                ConditionalPlugins<TPlugin, TContext> plugins, 
                Func<TContext, bool> defaultPredicate)
            {
                _dsl = dsl;
                _plugins = plugins;
                _defaultPredicate = defaultPredicate;
            }

            public ReplaceAppendPrependDsl With<TReplacement>(
                Func<TContext, bool> predicate = null, bool @default = false)
                where TReplacement : TPlugin
            {
                return With(ConditionalPlugin<TPlugin, TContext>
                    .Create<TReplacement>(predicate ?? _defaultPredicate, 
                        _plugins.Singleton), @default);
            }

            public ReplaceAppendPrependDsl With<TReplacement>(
                TReplacement instance, Func<TContext, bool> predicate = null, 
                bool dispose = false, bool @default = false)
                where TReplacement : TPlugin
            {
                return With(ConditionalPlugin<TPlugin, TContext>
                    .Create(instance, predicate ?? _defaultPredicate, dispose), @default);
            }

            private ReplaceAppendPrependDsl With(
                ConditionalPlugin<TPlugin, TContext> plugin, bool @default = false)
            {
                var exists = _plugins.AnyOfType<TReplace>();
                if (exists) _plugins.ReplaceAllOfTypeWith<TReplace>(plugin, @default);
                return new ReplaceAppendPrependDsl(_dsl, _plugins, exists, plugin, @default);
            }
        }

        public class ReplaceAppendPrependDsl
        {
            private readonly ConditionalPluginsDsl<TPlugin, TContext> _dsl;
            private readonly ConditionalPlugins<TPlugin, TContext> _plugins;
            private readonly bool _existed;
            private readonly ConditionalPlugin<TPlugin, TContext> _plugin;
            private readonly bool _default;

            public ReplaceAppendPrependDsl(ConditionalPluginsDsl<TPlugin, TContext> dsl,
                ConditionalPlugins<TPlugin, TContext> plugins, bool existed,
                ConditionalPlugin<TPlugin, TContext> plugin, bool @default)
            {
                _dsl = dsl;
                _plugins = plugins;
                _existed = existed;
                _plugin = plugin;
                _default = @default;
            }

            public ConditionalPluginsDsl<TPlugin, TContext> OrPrepend()
            {
                if (!_existed) _plugins.Prepend(_plugin, _default);
                return _dsl;
            }

            public ConditionalPluginsDsl<TPlugin, TContext> OrAppend()
            {
                if (!_existed) _plugins.Append(_plugin, _default); 
                return _dsl;
            }
        }

        public AppendDsl Append<TAppend>(
            Func<TContext, bool> predicate = null, bool @default = false)
            where TAppend : TPlugin
        {
            return Append(ConditionalPlugin<TPlugin, TContext>
                .Create<TAppend>(predicate ?? DefaultPredicate, 
                    Plugins.Singleton), @default);
        }

        public AppendDsl Append<TAppend>(
            TAppend instance, Func<TContext, bool> predicate = null, 
            bool dispose = false, bool @default = false)
            where TAppend : TPlugin
        {
            return Append(ConditionalPlugin<TPlugin, TContext>
                .Create(instance, predicate ?? DefaultPredicate, dispose), @default);
        }

        private AppendDsl Append(ConditionalPlugin<TPlugin, TContext> plugin, bool @default = false)
        {
            Plugins.Append(plugin, @default);
            return new AppendDsl(plugin, Plugins, DefaultPredicate);
        }

        public class AppendDsl : ConditionalPluginsDsl<TPlugin, TContext>
        {
            private readonly ConditionalPlugin<TPlugin, TContext> _plugin;
            private readonly ConditionalPlugins<TPlugin, TContext> _plugins;
            private readonly Func<TContext, bool> _defaultPredicate;

            public AppendDsl(ConditionalPlugin<TPlugin, TContext> plugin,
                ConditionalPlugins<TPlugin, TContext> plugins, 
                Func<TContext, bool> defaultPredicate) : base(plugins, defaultPredicate)
            {
                _plugin = plugin;
                _plugins = plugins;
                _defaultPredicate = defaultPredicate;
            }

            public AppendOrPrependDsl After<TFind>()
                where TFind : TPlugin
            {
                _plugins.Remove(_plugin);
                var exists = _plugins.AnyOfType<TFind>();
                if (exists) _plugins.AppendAfter<TFind>(_plugin);
                return new AppendOrPrependDsl(_plugin, _plugins, exists, _defaultPredicate);
            }
        }

        public class AppendOrPrependDsl : ConditionalPluginsDsl<TPlugin, TContext>
        {
            private readonly ConditionalPlugin<TPlugin, TContext> _plugin;
            private readonly ConditionalPlugins<TPlugin, TContext> _plugins;
            private readonly bool _existed;

            public AppendOrPrependDsl(ConditionalPlugin<TPlugin, TContext> plugin,
                ConditionalPlugins<TPlugin, TContext> plugins, bool existed,
                Func<TContext, bool> defaultPredicate) : base(plugins, defaultPredicate)
            {
                _plugin = plugin;
                _plugins = plugins;
                _existed = existed;
            }

            public ConditionalPluginsDsl<TPlugin, TContext> OrPrepend()
            {
                if (!_existed) _plugins.Prepend(_plugin);
                return this;
            }

            public ConditionalPluginsDsl<TPlugin, TContext> OrAppend()
            {
                if (!_existed) _plugins.Append(_plugin);
                return this;
            }
        }

        public PrependDsl Prepend<TPrepend>(
            Func<TContext, bool> predicate = null, bool @default = false) 
            where TPrepend : TPlugin
        {
            return Prepend(ConditionalPlugin<TPlugin, TContext>
                .Create<TPrepend>(predicate ?? DefaultPredicate, Plugins.Singleton), @default);
        }

        public PrependDsl Prepend<TPrepend>(
            TPrepend instance, Func<TContext, bool> predicate = null, 
            bool dispose = false, bool @default = false) 
            where TPrepend : TPlugin
        {
            return Prepend(ConditionalPlugin<TPlugin, TContext>
                .Create(instance, predicate ?? DefaultPredicate, dispose), @default);
        }

        private PrependDsl Prepend(
            ConditionalPlugin<TPlugin, TContext> plugin, bool @default = false)
        {
            Plugins.Prepend(plugin, @default);
            return new PrependDsl(plugin, Plugins, DefaultPredicate);
        }

        public class PrependDsl : ConditionalPluginsDsl<TPlugin, TContext>
        {
            private readonly ConditionalPlugin<TPlugin, TContext> _plugin;
            private readonly ConditionalPlugins<TPlugin, TContext> _plugins;
            private readonly Func<TContext, bool> _defaultPredicate;

            public PrependDsl(ConditionalPlugin<TPlugin, TContext> plugin,
                ConditionalPlugins<TPlugin, TContext> plugins,
                Func<TContext, bool> defaultPredicate) : base(plugins, defaultPredicate)
            {
                _plugin = plugin;
                _plugins = plugins;
                _defaultPredicate = defaultPredicate;
            }

            public PrependOrAppendDsl<TFind> Before<TFind>()
                where TFind : TPlugin
            {
                _plugins.Remove(_plugin);
                var exists = _plugins.AnyOfType<TFind>();
                if (exists) _plugins.PrependBefore<TFind>(_plugin);
                return new PrependOrAppendDsl<TFind>(_plugin, _plugins, exists, _defaultPredicate);
            }
        }

        public class PrependOrAppendDsl<TFind> : ConditionalPluginsDsl<TPlugin, TContext>
                where TFind : TPlugin
        {
            private readonly ConditionalPlugin<TPlugin, TContext> _plugin;
            private readonly ConditionalPlugins<TPlugin, TContext> _plugins;
            private readonly bool _existed;

            public PrependOrAppendDsl(ConditionalPlugin<TPlugin, TContext> plugin,
                ConditionalPlugins<TPlugin, TContext> plugins, bool existed,
                Func<TContext, bool> defaultPredicate) : base(plugins, defaultPredicate)
            {
                _plugin = plugin;
                _plugins = plugins;
                _existed = existed;
            }

            public ConditionalPluginsDsl<TPlugin, TContext> OrPrepend()
            {
                if (!_existed) _plugins.Prepend(_plugin); 
                return this;
            }

            public ConditionalPluginsDsl<TPlugin, TContext> OrAppend()
            {
                if (!_existed) _plugins.Append(_plugin);
                return this;
            }
        }

        public ConditionalPluginsDsl<TPlugin, TContext> When(Func<TContext, bool> predicate, 
            Action<ConditionalPluginsDsl<TPlugin, TContext>> configure)
        {
            configure(new ConditionalPluginsDsl<TPlugin, TContext>(Plugins, predicate));
            return this;
        }
    }
}
