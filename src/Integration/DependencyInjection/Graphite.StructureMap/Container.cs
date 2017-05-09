using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dependencies;
using Graphite.DependencyInjection;
using StructureMap;
using IContainer = Graphite.DependencyInjection.IContainer;
using StructureMapContainer = StructureMap.Container;
using StructureMapIContainer = StructureMap.IContainer;

namespace Graphite.StructureMap
{
    public class Container : IContainer, IDependencyResolver
    {
        private readonly StructureMapIContainer _container;

        public Container()
        {
            _container = new StructureMapContainer();
        }

        public Container(StructureMapIContainer container)
        {
            _container = container;
        }

        public Container(Action<ConfigurationExpression> configure)
        {
            _container = new StructureMapContainer(configure);
        }

        public IContainer CreateScopedContainer()
        {
            return new Container(_container.GetNestedContainer());
        }

        public object GetInstance(Type type, params Dependency[] dependencies)
        {
            return dependencies != null && dependencies.Any() 
                ? _container.GetInstance(type, dependencies.ToExplicitArgs())
                : _container.GetInstance(type);
        }

        public IEnumerable<object> GetInstances(Type type)
        {
            return _container.GetAllInstances(type).Cast<object>();
        }

        public void Register(Type plugin, Type concrete, bool singleton)
        {
            _container.Configure(x =>
            {
                if (singleton) x.ForSingletonOf(plugin).Use(concrete);
                else x.For(plugin).Use(concrete);
            });
        }

        public void Register(Type plugin, object instance, bool dispose)
        {
            _container.Configure(x => x.For(plugin).Use(instance, dispose));
        }

        public string GetConfiguration()
        {
            return _container.WhatDoIHave();
        }

        // Dependency Resolver

        IDependencyScope IDependencyResolver.BeginScope()
        {
            return (IDependencyScope)CreateScopedContainer();
        }

        object IDependencyScope.GetService(Type serviceType)
        {
            return _container.TryGetInstance(serviceType);
        }

        IEnumerable<object> IDependencyScope.GetServices(Type serviceType)
        {
            return GetInstances(serviceType);
        }

        public void Dispose()
        {
            _container.Dispose();
        }
    }
}
