using System;
using System.Collections;
using System.Collections.Generic;

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

            public Registration(Type pluginType, object instance)
            {
                PluginType = pluginType;
                ConcreteType = instance?.GetType();
                Instance = instance;
            }
            
            public Type PluginType { get; }
            public Type ConcreteType { get; }
            public bool IsInstance => Instance != null;
            public object Instance { get; }
            public bool Singleton { get; }
        }

        private readonly IList<Registration> _registrations;

        public Registry()
        {
            _registrations = new List<Registration>();
        }

        public Registry(Registry registry)
        {
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

        public Registry Register(Type plugin, object instance)
        {
            _registrations.Add(new Registration(plugin, instance));
            return this;
        }

        public Registry Register<TPlugin>(Type concrete, bool singleton = false) where TPlugin : class
        {
            return Register(typeof(TPlugin), concrete, singleton);
        }

        public Registry Register<TPlugin>(TPlugin instance) where TPlugin : class
        {
            return Register(typeof(TPlugin), instance);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<Registration> GetEnumerator()
        {
            return _registrations.GetEnumerator();
        }
    }
}
