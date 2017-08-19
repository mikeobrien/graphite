using System;
using System.Collections.Generic;
using Graphite.Reflection;

namespace Graphite.DependencyInjection
{
    public interface ITrackingContainer
    {
        Registry Registry { get; }
    }

    public class TrackingContainer : IContainer, ITrackingContainer
    {
        private readonly IContainer _container;
        private readonly ITypeCache _typeCache;

        public TrackingContainer(IContainer container, 
            ITypeCache typeCache, IContainer parent = null)
        {
            _container = container;
            _typeCache = typeCache;
            Parent = parent;
            Registry = new Registry(typeCache);
        }

        public IContainer Parent { get; }
        public Registry Registry { get; }

        public void Register(Type plugin, Type concrete, bool singleton)
        {
            Registry.Register(plugin, concrete, singleton);
            _container.Register(plugin, concrete, singleton);
        }

        public void Register(Type plugin, object instance, bool dispose)
        {
            Registry.Register(plugin, instance, dispose);
            _container.Register(plugin, instance, dispose);
        }

        public string GetConfiguration()
        {
            return _container.GetConfiguration();
        }
        
        public IContainer CreateScopedContainer()
        {
            return new TrackingContainer(_container.CreateScopedContainer(), _typeCache, this);
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
