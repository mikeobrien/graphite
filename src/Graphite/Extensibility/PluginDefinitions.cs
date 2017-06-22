using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Graphite.Extensibility
{
    public class PluginDefinitions<TPlugin, TContext> : IEnumerable<PluginDefinition<TPlugin, TContext>> 
    {
        public class State
        {
            public List<PluginDefinition<TPlugin, TContext>> Definitions;
            public bool Singleton;
            public Type Default;
        }

        private readonly State _state;
        private readonly Func<TContext, bool> _defaultPredicate;

        public PluginDefinitions(bool singleton)
        {
            _state = new State
            {
                Definitions = new List<PluginDefinition<TPlugin, TContext>>(),
                Singleton = singleton
            };
        }

        private PluginDefinitions(State state, Func<TContext, bool> defaultPredicate)
        {
            _state = state;
            _defaultPredicate = defaultPredicate;
        }

        public static PluginDefinitions<TPlugin, TContext> Create(
            Action<PluginDefinitions<TPlugin, TContext>> config = null, bool singleton = false)
        {
            var plugins = new PluginDefinitions<TPlugin, TContext>(singleton);
            config?.Invoke(plugins);
            return plugins;
        }

        public IEnumerable<PluginDefinition<TPlugin, TContext>> ThatApply(TContext context)
        {
            return _state.Definitions
                .Where(x => x.AppliesTo?.Invoke(context) ?? true)
                .OrderBy(x => _state.Definitions.IndexOf(x)).ToList();
        }

        public int Order(object instance)
        {
            return Order(GetFirst(instance));
        }

        public int Order(PluginDefinition<TPlugin, TContext> definition)
        {
            var order = _state.Definitions.IndexOf(definition);
            return order >= 0 ? order : short.MaxValue;
        }

        public PluginDefinitions<TPlugin, TContext> DefaultIs<TConcrete>() 
            where TConcrete : TPlugin
        {
            _state.Default = typeof(TConcrete);
            return this;
        }

        public bool IsDefault(Type concreteType) 
        {
            return concreteType != null && _state.Default == concreteType;
        }

        public PluginDefinition<TPlugin, TContext> GetFirst<TConcrete>()
            where TConcrete : TPlugin
        {
            return GetFirst(typeof(TConcrete));
        }

        public PluginDefinition<TPlugin, TContext> GetFirst(object instance)
        {
            return _state.Definitions.FirstOrDefault(x => 
                (object)x.Instance == instance) ?? GetFirst(instance.GetType());
        }

        public PluginDefinition<TPlugin, TContext> GetFirst(Type type)
        {
            return _state.Definitions.FirstOrDefault(x => x.Type == type);
        }

        public PluginDefinition<TPlugin, TContext> GetLast<TConcrete>()
            where TConcrete : TPlugin
        {
            return GetLast(typeof(TConcrete));
        }

        public PluginDefinition<TPlugin, TContext> GetLast(object instance)
        {
            return _state.Definitions.LastOrDefault(x =>
                (object)x.Instance == instance) ?? GetLast(instance.GetType());
        }

        public PluginDefinition<TPlugin, TContext> GetLast(Type type)
        {
            return _state.Definitions.LastOrDefault(x => x.Type == type);
        }

        public PluginDefinitions<TPlugin, TContext> Clear()
        {
            _state.Definitions.Clear();
            return this;
        }

        public bool Exists<TConcrete>() where TConcrete : TPlugin
        {
            return Exists(typeof(TConcrete));
        }

        public bool Exists(Type type)
        {
            return _state.Definitions.Any(x => x.Type == type);
        }

        public PluginDefinitions<TPlugin, TContext> Remove<TConcrete>(
            bool typeOnly = false) where TConcrete : TPlugin
        {
            var remove = _state.Definitions.Where(x => (!typeOnly || !x.HasInstance) && 
                x.Type == typeof(TConcrete)).ToList();
            remove.ForEach(x => _state.Definitions.Remove(x));
            return this;
        }

        public PluginDefinitions<TPlugin, TContext> Remove(PluginDefinition<TPlugin, TContext> plugin) 
        {
            _state.Definitions.Remove(plugin);
            return this;
        }

        public ReplaceDsl<TReplace> Replace<TReplace>()
            where TReplace : TPlugin
        {
            return new ReplaceDsl<TReplace>(this, _state.Singleton, _defaultPredicate);
        }

        public class ReplaceDsl<TReplace> where TReplace : TPlugin
        {
            private readonly PluginDefinitions<TPlugin, TContext> _plugins;
            private readonly bool _singleton;
            private readonly Func<TContext, bool> _defaultPredicate;

            public ReplaceDsl(PluginDefinitions<TPlugin, TContext> plugins, bool singleton, 
                Func<TContext, bool> defaultPredicate)
            {
                _plugins = plugins;
                _singleton = singleton;
                _defaultPredicate = defaultPredicate;
            }

            public PluginDefinitions<TPlugin, TContext> With<TReplacement>(
                Func<TContext, bool> predicate = null, bool @default = false)
                where TReplacement : TPlugin
            {
                return With<TReplacement>(PluginDefinition<TPlugin, TContext>
                    .Create<TReplacement>(predicate ?? _defaultPredicate, _singleton), @default);
            }

            public PluginDefinitions<TPlugin, TContext> With<TReplacement>(
                TReplacement instance, Func<TContext, bool> predicate = null, 
                bool dispose = false, bool @default = false)
                where TReplacement : TPlugin
            {
                return With<TReplacement>(PluginDefinition<TPlugin, TContext>
                    .Create(instance, predicate ?? _defaultPredicate, dispose), @default);
            }

            public PluginDefinitions<TPlugin, TContext> With<TReplacement>(
                PluginDefinition<TPlugin, TContext> plugin, bool @default = false)
                where TReplacement : TPlugin
            {
                _plugins.Remove(plugin);
                _plugins.Append<TReplacement>(plugin).After<TReplace>();
                _plugins.Remove<TReplace>();
                if (@default) _plugins.DefaultIs<TReplace>();
                return _plugins;
            }
        }

        public AppendDsl<TAppend> Append<TAppend>(
            Func<TContext, bool> predicate = null, bool @default = false)
            where TAppend : TPlugin
        {
            return Append<TAppend>(PluginDefinition<TPlugin, TContext>
                .Create<TAppend>(predicate ?? _defaultPredicate, _state.Singleton), @default);
        }

        public AppendDsl<TAppend> Append<TAppend>(
        TAppend instance, Func<TContext, bool> predicate = null, 
        bool dispose = false, bool @default = false)
            where TAppend : TPlugin
        {
            return Append<TAppend>(PluginDefinition<TPlugin, TContext>
                .Create(instance, predicate ?? _defaultPredicate, dispose), @default);
        }

        public AppendDsl<TAppend> Append<TAppend>(
            PluginDefinition<TPlugin, TContext> plugin, bool @default = false)
            where TAppend : TPlugin
        {
            if (!plugin.HasInstance) Remove<TAppend>(true);
            _state.Definitions.Remove(plugin);
            _state.Definitions.Add(plugin);
            if (@default) DefaultIs<TAppend>();
            return new AppendDsl<TAppend>(plugin, _state, _defaultPredicate);
        }

        public class AppendDsl<TAppend> : PluginDefinitions<TPlugin, TContext> where TAppend : TPlugin
        {
            private readonly PluginDefinition<TPlugin, TContext> _plugin;

            public AppendDsl(PluginDefinition<TPlugin, TContext> plugin, 
                State state, Func<TContext, bool> defaultPredicate) : base(state, defaultPredicate)
            {
                _plugin = plugin;
            }

            public PluginDefinitions<TPlugin, TContext> AfterOrPrepend<TFind>()
                where TFind : TPlugin
            {
                return Exists<TFind>() ? After<TFind>() : Prepend<TAppend>(_plugin);
            }

            public PluginDefinitions<TPlugin, TContext> After<TFind>()
                where TFind : TPlugin
            {
                if (!Exists<TFind>()) return this;
                var order = Order(GetLast<TFind>()) + 1;
                if (order >= _state.Definitions.Count) return this;
                _state.Definitions.Remove(_plugin);
                _state.Definitions.Insert(order, _plugin);
                return this;
            }
        }

        public PrependDsl<TPrepend> Prepend<TPrepend>(
            Func<TContext, bool> predicate = null, bool @default = false) 
            where TPrepend : TPlugin
        {
            return Prepend<TPrepend>(PluginDefinition<TPlugin, TContext>
                .Create<TPrepend>(predicate ?? _defaultPredicate, _state.Singleton), @default);
        }

        public PrependDsl<TPrepend> Prepend<TPrepend>(
            TPrepend instance, Func<TContext, bool> predicate = null, 
            bool dispose = false, bool @default = false) 
            where TPrepend : TPlugin
        {
            return Prepend<TPrepend>(PluginDefinition<TPlugin, TContext>
                .Create(instance, predicate ?? _defaultPredicate, dispose), @default);
        }

        public PrependDsl<TPrepend> Prepend<TPrepend>(
            PluginDefinition<TPlugin, TContext> plugin, bool @default = false)
            where TPrepend : TPlugin
        {
            if (!plugin.HasInstance) Remove<TPrepend>(true);
            _state.Definitions.Remove(plugin);
            _state.Definitions.Insert(0, plugin);
            if (@default) DefaultIs<TPrepend>();
            return new PrependDsl<TPrepend>(plugin, _state, _defaultPredicate);
        }

        public class PrependDsl<TPrepend> : PluginDefinitions<TPlugin, TContext> where TPrepend : TPlugin
        {
            private readonly PluginDefinition<TPlugin, TContext> _plugin;

            public PrependDsl(PluginDefinition<TPlugin, TContext> plugin, 
                State state, Func<TContext, bool> defaultPredicate) : base(state, defaultPredicate)
            {
                _plugin = plugin;
            }

            public PluginDefinitions<TPlugin, TContext> BeforeOrAppend<TFind>()
                where TFind : TPlugin
            {
                return Exists<TFind>() ? Before<TFind>() : Append<TPrepend>(_plugin);
            }

            public PluginDefinitions<TPlugin, TContext> Before<TFind>()
                where TFind : TPlugin
            {
                if (!Exists<TFind>()) return this;
                _state.Definitions.Remove(_plugin);
                _state.Definitions.Insert(Order(GetFirst<TFind>()), _plugin);
                return this;
            }
        }

        public PluginDefinitions<TPlugin, TContext> When(Func<TContext, bool> predicate, 
            Action<PluginDefinitions<TPlugin, TContext>> configure)
        {
            configure(new PluginDefinitions<TPlugin, TContext>(_state, predicate));
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<PluginDefinition<TPlugin, TContext>> GetEnumerator()
        {
            return _state.Definitions.GetEnumerator();
        }
    }
}
