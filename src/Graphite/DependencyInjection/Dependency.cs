using System;

namespace Graphite.DependencyInjection
{
    public class Dependency
    {
        public Dependency(Type type, object instance)
        {
            Type = type;
            Instance = instance;
        }

        public Type Type { get; }
        public object Instance { get; }

        public static Dependency For<T>(T instance)
        {
            return new Dependency(typeof(T), instance);
        }
    }
}
