using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Graphite.Extensibility
{
    public class PluginDefinitions<TPlugin, TContext> : IEnumerable<PluginDefinition<TPlugin, TContext>> 
    {
        private readonly List<PluginDefinition<TPlugin, TContext>> _definitions;
        private readonly bool _singleton;

        public PluginDefinitions(bool singleton)
        {
            _singleton = singleton;
            _definitions = new List<PluginDefinition<TPlugin, TContext>>();
        }

        private PluginDefinitions(List<PluginDefinition<TPlugin, TContext>> definitions, bool singleton)
        {
            _definitions = definitions;
            _singleton = singleton;
        }

        public static PluginDefinitions<TPlugin, TContext> Create(
            Action<PluginDefinitions<TPlugin, TContext>> config = null, bool singleton = false)
        {
            var plugins = new PluginDefinitions<TPlugin, TContext>(singleton);
            config?.Invoke(plugins);
            return plugins;
        }

        public IEnumerable<PluginDefinition<TPlugin, TContext>> ThatApplyTo(TContext context)
        {
            return _definitions
                .Where(x => x.AppliesTo?.Invoke(context) ?? true)
                .OrderBy(x => _definitions.IndexOf(x)).ToList();
        }

        public int Order(object instance)
        {
            return Order(instance.GetType());
        }

        public int Order<TConcrete>()
        {
            return Order(typeof(TConcrete));
        }

        public int Order(Type type)
        {
            return Order(Get(type));
        }

        public int Order(PluginDefinition<TPlugin, TContext> definition)
        {
            var order = _definitions.IndexOf(definition);
            return order >= 0 ? order : short.MaxValue;
        }

        public PluginDefinition<TPlugin, TContext> Get<TConcrete>()
        {
            return Get(typeof(TConcrete));
        }

        public PluginDefinition<TPlugin, TContext> Get(object instance)
        {
            return Get(instance.GetType());
        }

        public PluginDefinition<TPlugin, TContext> Get(Type type)
        {
            return _definitions.FirstOrDefault(x => x.Type == type);
        }

        public PluginDefinitions<TPlugin, TContext> Clear()
        {
            _definitions.Clear();
            return this;
        }

        public bool Exists<TConcrete>() where TConcrete : TPlugin
        {
            return Exists(typeof(TConcrete));
        }

        public bool Exists(Type type)
        {
            return _definitions.Any(x => x.Type == type);
        }

        public PluginDefinitions<TPlugin, TContext> Remove<TConcrete>() where TConcrete : TPlugin
        {
            var definition = Get<TConcrete>();
            if (definition != null) _definitions.Remove(definition);
            return this;
        }

        public ReplaceDsl<TReplace> Replace<TReplace>()
            where TReplace : TPlugin
        {
            return new ReplaceDsl<TReplace>(this, _singleton);
        }

        public class ReplaceDsl<TReplace> where TReplace : TPlugin
        {
            private readonly PluginDefinitions<TPlugin, TContext> _plugins;
            private readonly bool _singleton;

            public ReplaceDsl(PluginDefinitions<TPlugin, TContext> plugins, bool singleton)
            {
                _plugins = plugins;
                _singleton = singleton;
            }

            public PluginDefinitions<TPlugin, TContext> With<TReplacement>(
                Func<TContext, bool> predicate = null)
                where TReplacement : TPlugin
            {
                return With<TReplacement>(PluginDefinition<TPlugin, TContext>
                    .Create<TReplacement>(predicate, _singleton));
            }

            public PluginDefinitions<TPlugin, TContext> With<TReplacement>(
                TReplacement instance, Func<TContext, bool> predicate = null, bool dispose = false)
                where TReplacement : TPlugin
            {
                return With<TReplacement>(PluginDefinition<TPlugin, TContext>
                    .Create(instance, predicate, dispose));
            }

            public PluginDefinitions<TPlugin, TContext> With<TReplacement>(
                PluginDefinition<TPlugin, TContext> plugin)
                where TReplacement : TPlugin
            {
                _plugins.Append<TReplacement>(plugin).After<TReplace>();
                _plugins.Remove<TReplace>();
                return _plugins;
            }
        }

        public AppendDsl<TAppend> Append<TAppend>(
            Func<TContext, bool> predicate = null)
            where TAppend : TPlugin
        {
            return Append<TAppend>(PluginDefinition<TPlugin, TContext>
                .Create<TAppend>(predicate, _singleton));
        }

        public AppendDsl<TAppend> Append<TAppend>(
        TAppend instance, Func<TContext, bool> predicate = null, bool dispose = false)
            where TAppend : TPlugin
        {
            return Append<TAppend>(PluginDefinition<TPlugin, TContext>
                .Create(instance, predicate, dispose));
        }

        public AppendDsl<TAppend> Append<TAppend>(
            PluginDefinition<TPlugin, TContext> plugin)
            where TAppend : TPlugin
        {
            Remove<TAppend>();
            _definitions.Add(plugin);
            return new AppendDsl<TAppend>(_definitions, plugin, _singleton);
        }

        public class AppendDsl<TAppend> : PluginDefinitions<TPlugin, TContext> where TAppend : TPlugin
        {
            private readonly PluginDefinition<TPlugin, TContext> _plugin;

            public AppendDsl(
                List<PluginDefinition<TPlugin, TContext>> definitions,
                PluginDefinition<TPlugin, TContext> plugin, bool singleton) : base(definitions, singleton)
            {
                _plugin = plugin;
            }

            public PluginDefinitions<TPlugin, TContext> After<TFind>()
                where TFind : TPlugin
            {
                if (!Exists<TFind>()) return this;
                var order = Order<TFind>() + 1;
                if (order >= _definitions.Count) return this;
                Remove<TAppend>();
                _definitions.Insert(order, _plugin);
                return this;
            }
        }

        public PrependDsl<TPrepend> Prepend<TPrepend>(
            Func<TContext, bool> predicate = null) 
            where TPrepend : TPlugin
        {
            return Prepend<TPrepend>(PluginDefinition<TPlugin, TContext>
                    .Create<TPrepend>(predicate, _singleton));
        }

        public PrependDsl<TPrepend> Prepend<TPrepend>(
            TPrepend instance, Func<TContext, bool> predicate = null, bool dispose = false) 
            where TPrepend : TPlugin
        {
            return Prepend<TPrepend>(PluginDefinition<TPlugin, TContext>
                .Create(instance, predicate, dispose));
        }

        public PrependDsl<TPrepend> Prepend<TPrepend>(
            PluginDefinition<TPlugin, TContext> plugin)
            where TPrepend : TPlugin
        {
            Remove<TPrepend>();
            _definitions.Insert(0, plugin);
            return new PrependDsl<TPrepend>(_definitions, plugin, _singleton);
        }

        public class PrependDsl<TPrepend> : PluginDefinitions<TPlugin, TContext> where TPrepend : TPlugin
        {
            private readonly PluginDefinition<TPlugin, TContext> _plugin;

            public PrependDsl(
                List<PluginDefinition<TPlugin, TContext>> definitions,
                PluginDefinition<TPlugin, TContext> plugin, bool singleton) : base(definitions, singleton)
            {
                _plugin = plugin;
            }

            public PluginDefinitions<TPlugin, TContext> Before<TFind>()
                where TFind : TPlugin
            {
                Remove<TPrepend>();
                if (!Exists<TFind>()) return Append<TPrepend>(_plugin);
                _definitions.Insert(Order<TFind>(), _plugin);
                return this;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<PluginDefinition<TPlugin, TContext>> GetEnumerator()
        {
            return _definitions.GetEnumerator();
        }
    }
}
