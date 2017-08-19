using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Graphite.Extensions;
using Graphite.Reflection;

namespace Graphite.DependencyInjection
{
    public class Registry : IEnumerable<Registry.Registration>
    {
        public class Registration
        {
            public Registration(Type pluginType, Type concreteType, bool singleton)
            {
                PluginType = pluginType;
                ConcreteType = concreteType;
                Singleton = singleton;
            }

            public Registration(Type pluginType, object instance, bool dispose)
            {
                PluginType = pluginType;
                ConcreteType = instance?.GetType();
                Instance = instance;
                Dispose = dispose;
            }
            
            public Type PluginType { get; }

            public Type ConcreteType { get; }
            public bool Singleton { get; }

            public bool IsInstance => Instance != null;
            public object Instance { get; }
            public bool Dispose { get; }
        }

        private readonly ITypeCache _typeCache;
        private readonly IList<Registration> _registrations;

        public Registry(ITypeCache typeCache)
        {
            _typeCache = typeCache;
            _registrations = new List<Registration>();
        }

        public Registry(Registry registry, ITypeCache typeCache)
        {
            _typeCache = typeCache;
            _registrations = new List<Registration>(registry);
        }

        public Registry Register<TPlugin, TConcrete>(bool singleton = false) 
            where TPlugin : class where TConcrete : TPlugin
        {
            return Register(typeof(TPlugin), typeof(TConcrete), singleton);
        }

        public Registry Register(Type plugin, Type concrete, bool singleton = false)
        {
            _registrations.Add(new Registration(plugin, concrete, singleton));
            return this;
        }

        public Registry Register(Type plugin, object instance, bool dispose)
        {
            _registrations.Add(new Registration(plugin, instance, dispose));
            return this;
        }

        public Registry Register<TPlugin>(Type concrete, bool singleton = false) where TPlugin : class
        {
            return Register(typeof(TPlugin), concrete, singleton);
        }

        public Registry Register<TPlugin>(TPlugin instance) where TPlugin : class
        {
            return Register(typeof(TPlugin), instance, false);
        }

        public Registry Register<TPlugin>(TPlugin instance, bool dispose) where TPlugin : class, IDisposable
        {
            return Register(typeof(TPlugin), instance, dispose);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<Registration> GetEnumerator()
        {
            return _registrations.GetEnumerator();
        }

        public override string ToString()
        {
            return _registrations.OrderBy(r => r.PluginType.FullName).Select(r =>
            {
                var pluginType = _typeCache.GetTypeDescriptor(r.PluginType);
                var concreteType = r.ConcreteType != null ?
                    _typeCache.GetTypeDescriptor(r.ConcreteType) : null;

                return new
                {
                    PluginType = pluginType.FriendlyFullName,
                    PluginAssembly = pluginType.Type.Assembly.GetFriendlyName(),
                    r.Singleton,
                    Instance = r.Instance?.GetType().GetFriendlyTypeName(true),
                    ConcreteType = concreteType?.FriendlyFullName,
                    ConcreteAssembly = concreteType?.Type.Assembly.GetFriendlyName()
                };
            })
            .ToTable(x => new
            {
                Plugin_Type = x.PluginType,
                Plugin_Assembly = x.PluginAssembly,
                x.Singleton,
                x.Instance,
                Concrete_Type = x.ConcreteType,
                Concrete_Assembly = x.ConcreteAssembly
            });
        }
    }
}
