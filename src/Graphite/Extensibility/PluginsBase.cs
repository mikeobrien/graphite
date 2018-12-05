using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Graphite.Extensibility
{
    public abstract class PluginsBase<TPlugin, T> : IEnumerable<T>
        where T : Plugin<TPlugin>
    {
        protected readonly List<T> Plugins;
        protected T Default;

        protected PluginsBase(bool singleton)
        {
            Singleton = singleton;
            Plugins = new List<T>();
        }

        protected PluginsBase(IEnumerable<T> plugins, bool singleton, T @default)
        {
            Singleton = singleton;
            Plugins = new List<T>(plugins);
            Default = @default;
        }
        
        public bool Singleton { get; }

        public int IndexOf(object instance)
        {
            return IndexOf(InstanceOrTypePluginFor(instance));
        }

        public int IndexOf(T plugin)
        {
            var order = Plugins.IndexOf(plugin);
            return order >= 0 ? order : short.MaxValue;
        }

        public PluginsBase<TPlugin, T> DefaultIs(T plugin) 
        {
            Default = plugin;
            return this;
        }

        public bool IsDefault(T plugin)
        {
            return plugin.Equals(Default);
        }
        
        public TPlugin GetDefaultInstance(
            IEnumerable<TPlugin> plugins)
        {
            return PluginsFor(plugins)
                .Where(x => x.IsDefault)
                .Select(x => x.Instance)
                .FirstOrDefault();
        }

        public T InstancePluginFor(object instance)
        {
            return Plugins.FirstOrDefault(x =>
                x.HasInstance && (object)x.Instance == instance);
        }

        public T TypePluginFor<TConcrete>()
        {
            return TypePluginFor(typeof(TConcrete));
        }

        public T TypePluginFor(Type type)
        {
            return Plugins.FirstOrDefault(x => 
                !x.HasInstance && x.Type == type);
        }

        public T InstanceOrTypePluginFor(object instance)
        {
            return InstancePluginFor(instance) ?? 
                TypePluginFor(instance?.GetType());
        }

        public class PluginInstance
        {
            public PluginInstance(TPlugin instance, T plugin, bool @default)
            {
                Instance = instance;
                Plugin = plugin;
                IsDefault = @default;
            }

            public TPlugin Instance { get; }
            public T Plugin { get; }
            public bool IsDefault { get; }
        }

        public IEnumerable<PluginInstance> PluginsFor(IEnumerable<TPlugin> plugins)
        {
            return plugins
                .Where(x => x != null)
                .Select(x =>
                {
                    var plugin = InstanceOrTypePluginFor(x);
                    return new
                    {
                        Instance = x,
                        Plugin = plugin,
                        Order = IndexOf(plugin)
                    };
                })
                .Where(x => x.Plugin != null)
                .OrderBy(x => x.Order)
                .Select(x => new PluginInstance(x.Instance, 
                    x.Plugin, IsDefault(x.Plugin)))
                .ToList();
        }

        public IEnumerable<T> AllOfType<TConcrete>()
        {
            return AllOfType(typeof(TConcrete));
        }

        public IEnumerable<T> AllOfType(Type type)
        {
            return Plugins.Where(x => x.Type == type);
        }

        public T FirstOfType<TConcrete>()
            where TConcrete : TPlugin
        {
            return FirstOfType(typeof(TConcrete));
        }

        public T FirstOfType(Type type)
        {
            return Plugins.FirstOrDefault(x => x.Type == type);
        }

        public T LastOfType<TConcrete>()
            where TConcrete : TPlugin
        {
            return LastOfType(typeof(TConcrete));
        }

        public T LastOfType(Type type)
        {
            return Plugins.LastOrDefault(x => x.Type == type);
        }

        public PluginsBase<TPlugin, T> Clear()
        {
            Plugins.Clear();
            return this;
        }

        public PluginsBase<TPlugin, T> 
            ClearExcept<TConcrete>() where TConcrete : TPlugin
        {
            Remove(Plugins.Where(x => x.Type != typeof(TConcrete)));
            return this;
        }

        public bool AnyOfType<TConcrete>() where TConcrete : TPlugin
        {
            return AnyOfType(typeof(TConcrete));
        }

        public bool AnyOfType(Type type)
        {
            return Plugins.Any(x => x.Type == type);
        }

        public PluginsBase<TPlugin, T> Remove(IEnumerable<T> plugins)
        {
            plugins.ToList().ForEach(x => Remove(x));
            return this;
        }

        public PluginsBase<TPlugin, T> Remove(T plugin)
        {
            if (Plugins.Contains(plugin)) Plugins.RemoveAll(d => d.Equals(plugin));
            return this;
        }

        public PluginsBase<TPlugin, T> RemoveTypePlugin<TConcrete>() 
            where TConcrete : TPlugin
        {
            var remove = TypePluginFor<TConcrete>();
            if (remove != null) Remove(remove);
            return this;
        }

        public PluginsBase<TPlugin, T> RemoveInstancePlugin(object instance)
        {
            var remove = InstancePluginFor(instance);
            if (remove != null) Remove(remove);
            return this;
        }

        public PluginsBase<TPlugin, T> RemoveAllOfType<TConcrete>()
            where TConcrete : TPlugin
        {
            return Remove(AllOfType<TConcrete>());
        }

        public PluginsBase<TPlugin, T> ReplaceTypePluginWithPrepend<TReplace>(
            T pluging, bool @default = false)
            where TReplace : TPlugin
        {
            return ReplaceWithOrPrepend(TypePluginFor<TReplace>(), pluging, @default);
        }

        public PluginsBase<TPlugin, T> ReplaceInstancePluginWithPrepend(
            object replace, T plugin, bool @default = false)
        {
            return ReplaceWithOrPrepend(InstancePluginFor(replace), plugin, @default);
        }

        public PluginsBase<TPlugin, T> ReplaceAllOfTypeWithOrPrepend<TReplace>(
            T plugin, bool @default = false)
            where TReplace : TPlugin
        {
            return ReplaceAllOfTypeWith<TReplace>(
                r => ReplaceWithOrPrepend(r, plugin, @default));
        }

        public PluginsBase<TPlugin, T> ReplaceTypePluginWithAppend<TReplace>(
            T pluging, bool @default = false)
            where TReplace : TPlugin
        {
            return ReplaceWithOrAppend(TypePluginFor<TReplace>(), pluging, @default);
        }

        public PluginsBase<TPlugin, T> ReplaceInstancePluginWithAppend(
            object replace, T plugin, bool @default = false)
        {
            return ReplaceWithOrAppend(InstancePluginFor(replace), plugin, @default);
        }

        public PluginsBase<TPlugin, T> ReplaceAllOfTypeWith<TReplace>(
            T plugin, bool @default = false)
            where TReplace : TPlugin
        {
            return ReplaceAllOfTypeWith<TReplace>( 
                r => ReplaceWith(r, () => AppendAfter(plugin, r, @default)));
        }

        public PluginsBase<TPlugin, T> ReplaceAllOfTypeWithOrAppend<TReplace>(
            T plugin, bool @default = false)
            where TReplace : TPlugin
        {
            return ReplaceAllOfTypeWith<TReplace>( 
                r => ReplaceWithOrAppend(r, plugin, @default));
        }

        private PluginsBase<TPlugin, T> ReplaceAllOfTypeWith<TReplace>(Action<T> replace)
            where TReplace : TPlugin
        {
            var remove = AllOfType<TReplace>().ToList();
            replace(remove.LastOrDefault());
            Remove(remove);
            return this;
        }

        public PluginsBase<TPlugin, T> ReplaceWithOrPrepend(
            T replace, T plugin, bool @default = false)
        {
            return ReplaceWith(replace,
                () => AppendAfterOrPrepend(plugin, replace, @default),
                () => Prepend(plugin, @default));
        }

        public PluginsBase<TPlugin, T> ReplaceWithOrAppend(
            T replace, T plugin, bool @default = false)
        {
            return ReplaceWith(replace,
                () => AppendAfterOrAppend(plugin, replace, @default), 
                () => Append(plugin, @default));
        }

        private PluginsBase<TPlugin, T> ReplaceWith(T replace, 
            Action add, Action defaultAction = null)
        {
            if (replace != null)
            {
                add();
                Remove(replace);
            }
            else defaultAction?.Invoke();
            return this;
        }

        public PluginsBase<TPlugin, T> Append(T plugin, bool @default = false)
        {
            Remove(plugin);
            Plugins.Add(plugin);
            if (@default) DefaultIs(plugin);
            return this;
        }

        public PluginsBase<TPlugin, T> AppendAfter<TFind>(
            T plugin, bool @default = false)
            where TFind : TPlugin
        {
            return AppendAfter(plugin, LastOfType<TFind>(), @default);
        }
        
        public PluginsBase<TPlugin, T> AppendAfter(
            T plugin, T after, bool @default = false)
        {
            return AppendAfterOrDefault(plugin, after, @default);
        }

        public PluginsBase<TPlugin, T> AppendAfterOrAppend<TFind>(
            T plugin, bool @default = false)
            where TFind : TPlugin
        {
            return AppendAfterOrAppend(plugin, LastOfType<TFind>(), @default);
        }

        public PluginsBase<TPlugin, T> AppendAfterOrPrepend<TFind>(
            T plugin, bool @default = false)
            where TFind : TPlugin
        {
            return AppendAfterOrPrepend(plugin, LastOfType<TFind>(), @default);
        }

        public PluginsBase<TPlugin, T> AppendAfterOrAppend(
            T plugin, T after, bool @default = false)
        {
            return AppendAfterOrDefault(plugin, after,
                @default, (pds, pd, d) => pds.Append(pd, d));
        }

        public PluginsBase<TPlugin, T> AppendAfterOrPrepend(
            T plugin, T after, bool @default = false)
        {
            return AppendAfterOrDefault(plugin, after, 
                @default, (pds, pd, d) => pds.Prepend(pd, d));
        }

        private PluginsBase<TPlugin, T> AppendAfterOrDefault(
            T plugin, T after, bool @default, 
            Action<PluginsBase<TPlugin, T>, T, bool> defaultAction = null)
        {
            Remove(plugin);
            var index = Plugins.IndexOf(after) + 1;
            if (index == Plugins.Count) return Append(plugin, @default);
            if (index >= 1) return Insert(plugin, index, @default);
            defaultAction?.Invoke(this, plugin, @default);
            return this;
        }

        public PluginsBase<TPlugin, T> Prepend(
            T plugin, bool @default = false)
        {
            Remove(plugin);
            if (Plugins.Any()) Plugins.Insert(0, plugin);
            else Plugins.Add(plugin);
            if (@default) DefaultIs(plugin);
            return this;
        }

        public PluginsBase<TPlugin, T> PrependBefore<TFind>(
            T plugin, bool @default = false)
            where TFind : TPlugin
        {
            return PrependBefore(plugin, FirstOfType<TFind>(), @default);
        }

        public PluginsBase<TPlugin, T> PrependBefore(
            T plugin, T before, bool @default = false)
        {
            return PrependBeforeOrDefault(plugin, before, @default);
        }

        public PluginsBase<TPlugin, T> PrependBeforeOrAppend<TFind>(
            T plugin, bool @default = false)
            where TFind : TPlugin
        {
            return PrependBeforeOrAppend(plugin, FirstOfType<TFind>(), @default);
        }

        public PluginsBase<TPlugin, T> PrependBeforeOrPrepend<TFind>(
            T plugin, bool @default = false)
            where TFind : TPlugin
        {
            return PrependBeforeOrPrepend(plugin, FirstOfType<TFind>(), @default);
        }

        public PluginsBase<TPlugin, T> PrependBeforeOrAppend(
            T plugin, T before, bool @default = false)
        {
            return PrependBeforeOrDefault(plugin, before,
                @default, (pds, pd, d) => pds.Append(pd, d));
        }

        public PluginsBase<TPlugin, T> PrependBeforeOrPrepend(
            T plugin, T before, bool @default = false)
        {
            return PrependBeforeOrDefault(plugin, before,
                @default, (pds, pd, d) => pds.Prepend(pd, d));
        }

        private PluginsBase<TPlugin, T> PrependBeforeOrDefault(
            T plugin, T after, bool @default,
            Action<PluginsBase<TPlugin, T>, T, bool> defaultAction = null)
        {
            Remove(plugin);
            var index = Plugins.IndexOf(after);
            if (index >= 0) return Insert(plugin, index, @default);
            defaultAction?.Invoke(this, plugin, @default);
            return this;
        }

        public PluginsBase<TPlugin, T> Insert(
            T plugin, int index, bool @default)
        {
            Remove(plugin);
            Plugins.Insert(index, plugin);
            if (@default) DefaultIs(plugin);
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Plugins.GetEnumerator();
        }
    }
}
