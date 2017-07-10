using System;

namespace Graphite.Extensibility
{
    public class Plugin<TPlugin>
    {
        protected Plugin(Plugin<TPlugin> source)
        {
            Instance = source.Instance;
            Type = source.Type;
            Singleton = source.Singleton;
            Dispose = source.Dispose;
        }

        protected Plugin(Type concrete, TPlugin instance, bool dispose)
        {
            Instance = instance;
            Type = concrete;
            Dispose = dispose;
        }

        protected Plugin(Type concrete, bool singleton)
        {
            Singleton = singleton;
            Type = concrete;
        }

        public static Plugin<TPlugin> Create(bool singleton = false)
        {
            return new Plugin<TPlugin>(null, singleton);
        }

        public static Plugin<TPlugin> Create<TConcrete>(
            bool singleton = false) where TConcrete : TPlugin
        {
            return new Plugin<TPlugin>(typeof(TConcrete), singleton);
        }

        public static Plugin<TPlugin> Create<TConcrete>(
            TConcrete instance, bool dispose = false) 
            where TConcrete : TPlugin
        {
            return new Plugin<TPlugin>(typeof(TConcrete), instance, dispose);
        }

        public TPlugin Instance { get; protected set; }
        public bool HasInstance => Instance != null;
        public Type Type { get; protected set; }
        public bool Singleton { get; protected set; }
        public bool Dispose { get; protected set; }

        public void Set<TConcrete>(bool singleton = false) where TConcrete : TPlugin
        {
            Type = typeof(TConcrete);
            Singleton = singleton;
            Instance = default(TPlugin);
            Dispose = false;
        }

        public void Set<TConcrete>(TConcrete instance, 
            bool dispose = false) where TConcrete : TPlugin
        {
            Type = typeof(TConcrete);
            Instance = instance;
            Singleton = false;
            Dispose = dispose;
        }

        public Plugin<TPlugin> Clone()
        {
            return new Plugin<TPlugin>(this);
        }

        public override int GetHashCode()
        {
            return (HasInstance ? Instance.GetHashCode() : Type.GetHashCode()) ^ 
                typeof(Plugin<TPlugin>).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var compare = obj as Plugin<TPlugin>;
            return compare != null && (compare == this ||
                compare.GetHashCode() == GetHashCode());
        }

        public override string ToString()
        {
            return Type.Name;
        }
    }
}