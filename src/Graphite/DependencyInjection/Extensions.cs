using System;
using System.Collections.Generic;
using System.Linq;
using Graphite.Extensions;

namespace Graphite.DependencyInjection
{
    public static class Extensions
    {
        public static T GetInstance<T>(this IContainer container, 
            params Dependency[] dependencies) where T : class
        {
            return (T)container.GetInstance(typeof(T), dependencies);
        }

        public static T GetInstance<T>(this IContainer container,
            Type concreteType, params Dependency[] dependencies) where T : class
        {
            return (T)container.GetInstance(concreteType, dependencies);
        }

        public static IEnumerable<T> GetInstances<T>(this IContainer container) where T : class
        {
            return container.GetInstances(typeof(T)).Cast<T>();
        }

        public static IContainer Register<TPlugin>(this IContainer container,
            Type concrete, bool singleton) where TPlugin : class
        {
            container.Register(typeof(TPlugin), concrete, singleton);
            return container;
        }

        public static IContainer Register<TPlugin>(this IContainer container, 
            TPlugin instance) where TPlugin : class
        {
            container.Register(typeof(TPlugin), instance);
            return container;
        }

        public static IContainer IncludeRegistry(this IContainer container, Registry registry)
        {
            registry.ForEach(x =>
            {
                if (x.PluginType == null) return;
                if (x.IsInstance) container.Register(x.PluginType, x.Instance);
                else if (x.ConcreteType != null)
                    container.Register(x.PluginType, x.ConcreteType, x.Singleton);
            });
            return container;
        }
    }
}