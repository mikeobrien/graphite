using System;

namespace Graphite.Extensibility
{
    public class PluginDefinition<T>
    {
        public static PluginDefinition<T> Create<TConcrete>(bool singleton = false) where TConcrete : T
        {
            var plugin = new PluginDefinition<T>();
            plugin.Set<TConcrete>(singleton);
            return plugin;
        }

        public static PluginDefinition<T> Create<TConcrete>(TConcrete instance) where TConcrete : T
        {
            var plugin = new PluginDefinition<T>();
            plugin.Set(instance);
            return plugin;
        }

        public void Set<TConcrete>(bool singleton = false) where TConcrete : T
        {
            Type = typeof(TConcrete);
            Singleton = singleton;
            Instance = default(T);
        }

        public void Set<TConcrete>(TConcrete instance) where TConcrete : T
        {
            Type = typeof(TConcrete);
            Instance = instance;
            Singleton = false;
        }

        public T Instance { get; private set; }
        public bool HasInstance => Instance != null;
        public Type Type { get; private set; }
        public bool Singleton { get; private set; }
    }

    public class PluginDefinition<T, TContext>
    {
        public static PluginDefinition<T, TContext> Create<TConcrete>(
            Func<TContext, bool> predicate, bool singleton = false)
            where TConcrete : T
        {
            var plugin = new PluginDefinition<T, TContext>
            {
                Type = typeof(TConcrete),
                Singleton = singleton,
                AppliesTo = predicate
            };
            return plugin;
        }

        public static PluginDefinition<T, TContext> Create<TConcrete>(
            TConcrete instance, Func<TContext, bool> predicate)
            where TConcrete : T
        {
            var plugin = new PluginDefinition<T, TContext>
            {
                Type = typeof(TConcrete),
                Instance = instance,
                AppliesTo = predicate
            };
            return plugin;
        }

        public T Instance { get; private set; }
        public bool HasInstance => Instance != null;
        public Type Type { get; protected set; }
        public bool Singleton { get; protected set; }
        public Func<TContext, bool> AppliesTo { get; private set; }
    }
}