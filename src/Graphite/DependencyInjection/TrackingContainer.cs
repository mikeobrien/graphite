using System;
using System.Collections.Generic;
using System.Linq;
using Graphite.Extensions;
using Graphite.Reflection;

namespace Graphite.DependencyInjection
{
    public interface ITrackingContainer
    {
        Registry ParentRegistry { get; }
        Registry Registry { get; }
    }

    public class TrackingContainer : IContainer, ITrackingContainer
    {
        private readonly IContainer _container;

        public TrackingContainer(IContainer container)
        {
            _container = container;
            ParentRegistry = new Registry();
            Registry = new Registry();
        }

        public TrackingContainer(IContainer container, Registry registry)
        {
            _container = container;
            ParentRegistry = new Registry(registry);
            Registry = new Registry();
        }

        public Registry ParentRegistry { get; }
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
            return $"{_container.GetConfiguration()}\r\nRegistrations:\r\n\r\n{this}\r\n\r\n";
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

        public override string ToString()
        {
            var typeCache = _container.GetInstance<ITypeCache>();
            return Registry.OrderBy(r => r.PluginType.FullName).Select(r =>
            {
                var pluginType = typeCache.GetTypeDescriptor(r.PluginType);
                var concreteType = r.ConcreteType != null ? 
                    typeCache.GetTypeDescriptor(r.ConcreteType) : null;

                return new
                {
                    PluginType = pluginType.FriendlyFullName,
                    PluginAssembly = pluginType.Type.Assembly.GetFriendlyName(),
                    r.Singleton,
                    r.Instance,
                    ConcreteType = concreteType?.FriendlyFullName,
                    ConcreteAssembly = concreteType?.Type.Assembly.GetFriendlyName()
                };
            }).ToTable(x => new
            {
                Plugin_Type = x.PluginType,
                Plugin_Assembly = x.PluginAssembly,
                x.Singleton,
                x.Instance,
                Concrete_Type = x.ConcreteType,
                Concrete_Assembly = x.ConcreteAssembly
            });
        }

        public void Dispose()
        {
            _container.Dispose();
        }
    }
}
