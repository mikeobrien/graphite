using System;
using System.Collections.Generic;

namespace Graphite.DependencyInjection
{
    public class TrackingContainer : IContainer
    {
        private readonly IContainer _container;

        public TrackingContainer(IContainer container)
        {
            _container = container;
            Registry = new Registry();
        }

        public TrackingContainer(IContainer container, Registry registry)
        {
            _container = container;
            Registry = new Registry(registry);
        }

        public Registry Registry { get; }

        public void Register(Type plugin, Type concrete, bool singleton)
        {
            Registry.Register(plugin, concrete, singleton);
            _container.Register(plugin, concrete, singleton);
        }

        public void Register(Type plugin, object instance)
        {
            Registry.Register(plugin, instance);
            _container.Register(plugin, instance);
        }

        public IContainer CreateScopedContainer()
        {
            return new TrackingContainer(_container.CreateScopedContainer(), Registry);
        }

        public object GetInstance(Type type, params Dependency[] dependencies)
        {
            return _container.GetInstance(type, dependencies);
        }

        public IEnumerable<object> GetInstances(Type type)
        {
            return _container.GetInstances(type);
        }

        public void Dispose()
        {
            _container.Dispose();
        }
    }
}
