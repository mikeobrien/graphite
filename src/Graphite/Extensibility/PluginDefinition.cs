using System;

namespace Graphite.Extensibility
{
    public abstract class PluginDefinitionBase<T>
    {
        protected PluginDefinitionBase(Type concrete, T instance, bool dispose)
        {
            Instance = instance;
            Type = concrete;
            Dispose = dispose;
        }

        protected PluginDefinitionBase(Type concrete, bool singleton)
        {
            Singleton = singleton;
            Type = concrete;
        }

        public T Instance { get; protected set; }
        public bool HasInstance => Instance != null;
        public Type Type { get; protected set; }
        public bool Singleton { get; protected set; }
        public bool Dispose { get; protected set; }
    }

    public class PluginDefinition<T> : PluginDefinitionBase<T>
    {
        protected PluginDefinition(Type concrete, T instance, bool dispose) : base(concrete, instance, dispose) { }
        protected PluginDefinition(Type concrete, bool singleton) : base(concrete, singleton) { }

        public static PluginDefinition<T> Create<TConcrete>(bool singleton = false) where TConcrete : T
        {
            return new PluginDefinition<T>(typeof(TConcrete), singleton);
        }

        public static PluginDefinition<T> Create<TConcrete>(TConcrete instance, bool dispose = false) 
            where TConcrete : T
        {
            return new PluginDefinition<T>(typeof(TConcrete), instance, dispose);
        }

        public void Set<TConcrete>(bool singleton = false) where TConcrete : T
        {
            Type = typeof(TConcrete);
            Singleton = singleton;
            Instance = default(T);
        }

        public void Set<TConcrete>(TConcrete instance, bool dispose = false) where TConcrete : T
        {
            Type = typeof(TConcrete);
            Instance = instance;
            Singleton = false;
        }
    }

    public class PluginDefinition<T, TContext> : PluginDefinition<T>
    {
        private PluginDefinition(Type concrete, T instance, 
            Func<TContext, bool> appliesTo, bool dispose) : 
            base(concrete, instance, dispose)
        {
            AppliesTo = appliesTo;
        }

        private PluginDefinition(Type concrete, 
            Func<TContext, bool> appliesTo, bool singleton) : 
            base(concrete, singleton)
        {
            AppliesTo = appliesTo;
        }

        public static PluginDefinition<T, TContext> Create<TConcrete>(
            Func<TContext, bool> predicate, bool singleton = false)
            where TConcrete : T
        {
            return new PluginDefinition<T, TContext>( 
                typeof(TConcrete), predicate, singleton);
        }

        public static PluginDefinition<T, TContext> Create<TConcrete>(
            TConcrete instance, Func<TContext, bool> predicate, bool dispose = false)
            where TConcrete : T
        {
            return new PluginDefinition<T, TContext>(
                typeof(TConcrete), instance, predicate, dispose);
        }

        public Func<TContext, bool> AppliesTo { get; }
    }
}