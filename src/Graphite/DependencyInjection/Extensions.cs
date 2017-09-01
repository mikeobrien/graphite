using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Graphite.Extensions;
using Graphite.Linq;

namespace Graphite.DependencyInjection
{
    public static class Extensions
    {
        public static T GetInstance<T>(this IContainer container, object dependency,
            params object[] dependencies) where T : class
        {
            return (T)container.GetInstance(typeof(T), 
                dependency.Concat(dependencies).Select(Dependency.For).ToArray());
        }

        public static T GetInstance<T>(this IContainer container,
            params Dependency[] dependencies) where T : class
        {
            return (T)container.GetInstance(typeof(T), dependencies);
        }

        public static T GetInstance<T>(this IContainer container, object dependency, 
            Type concreteType, params object[] dependencies) where T : class
        {
            return container.GetInstance<T>(concreteType,
                dependency.Concat(dependencies).Select(Dependency.For).ToArray());
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
            container.Register(typeof(TPlugin), instance, false);
            return container;
        }

        public static IContainer Register<TPlugin>(this IContainer container,
            TPlugin instance, bool dispose) where TPlugin : class, IDisposable
        {
            container.Register(typeof(TPlugin), instance, dispose);
            return container;
        }

        public static IContainer Register(this IContainer container,
            Type pluginType, object instance, bool dispose)
        {
            container.Register(pluginType, instance, dispose);
            return container;
        }

        public static IContainer IncludeRegistry(this IContainer container, Registry registry)
        {
            registry.ForEach(x =>
            {
                if (x.PluginType == null) return;
                if (x.IsInstance) container.Register(x.PluginType, x.Instance, x.Dispose);
                else if (x.ConcreteType != null)
                    container.Register(x.PluginType, x.ConcreteType, x.Singleton);
            });
            return container;
        }

        private static readonly string ContainerProperty = typeof(IContainer).FullName;

        public static IContainer GetGraphiteContainer(this HttpRequestMessage request)
        {
            return request.Properties[ContainerProperty] as IContainer;
        }

        public static void SetGraphiteContainer(this HttpRequestMessage request, IContainer container)
        {
            request.Properties[ContainerProperty] = container;
        }
    }
}