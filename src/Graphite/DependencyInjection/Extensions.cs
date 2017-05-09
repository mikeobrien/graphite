using System;
using System.Collections.Generic;
using System.Linq;

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
    }
}