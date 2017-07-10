using System.Linq;
using Graphite.DependencyInjection;
using Graphite.Extensibility;
using Graphite.StructureMap;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Extensibility
{
    [TestFixture]
    public class ContainerExtensionTests
    {
        public interface IPluginType { }
        public class Plugin : IPluginType { }
        public class Plugin1 : IPluginType { }
        public class Plugin2 : IPluginType { }
        public class Context { }

        [Test]
        public void Should_register_plugins_in_container()
        {
            var instance = new Plugin1();
            var container = new Container();
            var plugin1 = Plugin<IPluginType>.Create(instance, true);
            var plugin2 = Plugin<IPluginType>.Create<Plugin2>(true);
            var plugins = new Plugins<IPluginType>(true);

            plugins.Append(plugin1).Append(plugin2);

            container.RegisterPlugins(plugins);

            var instances = container.GetInstances<IPluginType>();

            instances.Count().ShouldEqual(2);
            instances.ShouldContain(instance);
        }

        [Test]
        public void Should_register_conditional_plugins_in_container()
        {
            var instance = new Plugin1();
            var container = new Container();
            var plugin1 = ConditionalPlugin<IPluginType, Context>.Create(instance, x => false, true);
            var plugin2 = ConditionalPlugin<IPluginType, Context>.Create<Plugin2>(x => false, true);
            var plugins = new ConditionalPlugins<IPluginType, Context>(true);

            plugins.Append(plugin1).Append(plugin2);

            container.RegisterPlugins(plugins);

            var instances = container.GetInstances<IPluginType>();

            instances.Count().ShouldEqual(2);
            instances.ShouldContain(instance);
        }

        [Test]
        public void Should_register_plugin_instance_in_container()
        {
            var instance = new Plugin();
            var container = new Container();
            var plugin = Plugin<IPluginType>.Create(instance, true);

            container.RegisterPlugin(plugin);

            container.GetInstance<IPluginType>().ShouldEqual(instance);
        }

        [Test]
        public void Should_register_plugin_type_in_container()
        {
            var container = new Container();
            var plugin = Plugin<IPluginType>.Create<Plugin>(true);

            container.RegisterPlugin(plugin);
            
            container.GetInstance<IPluginType>().ShouldBeType<Plugin>();
        }

        [Test]
        public void Should_register_plugins_with_registry()
        {
            var instance = new Plugin1();
            var registry = new Registry();
            var plugin1 = Plugin<IPluginType>.Create(instance, true);
            var plugin2 = Plugin<IPluginType>.Create<Plugin2>(true);
            var plugins = new Plugins<IPluginType>(true);

            plugins.Append(plugin1).Append(plugin2);

            registry.RegisterPlugins(plugins);

            registry.Count().ShouldEqual(2);
            
            Should_be_plugin_instance(registry.First(), instance);
            Should_be_plugin_type<Plugin2>(registry.Second());
        }

        [Test]
        public void Should_register_conditional_plugins_with_registry()
        {
            var instance = new Plugin1();
            var registry = new Registry();
            var plugin1 = ConditionalPlugin<IPluginType, Context>.Create(instance, x => false, true);
            var plugin2 = ConditionalPlugin<IPluginType, Context>.Create<Plugin2>(x => false, true);
            var plugins = new ConditionalPlugins<IPluginType, Context>(true);

            plugins.Append(plugin1).Append(plugin2);

            registry.RegisterPlugins(plugins);

            registry.Count().ShouldEqual(2);

            Should_be_plugin_instance(registry.First(), instance);
            Should_be_plugin_type<Plugin2>(registry.Second());
        }

        [Test]
        public void Should_register_plugin_instance_with_registry()
        {
            var instance = new Plugin();
            var registry = new Registry();
            var plugin = Plugin<IPluginType>.Create(instance, true);

            registry.RegisterPlugin(plugin);

            registry.Count().ShouldEqual(1);

            Should_be_plugin_instance(registry.First(), instance);
        }

        [Test]
        public void Should_register_plugin_type_with_registry()
        {
            var registry = new Registry();
            var plugin = Plugin<IPluginType>.Create<Plugin>(true);

            registry.RegisterPlugin(plugin);

            registry.Count().ShouldEqual(1);

            Should_be_plugin_type<Plugin>(registry.First());
        }

        private void Should_be_plugin_type<T>(Registry.Registration registration)
        {
            registration.PluginType.ShouldEqual(typeof(IPluginType));
            registration.Singleton.ShouldBeTrue();
            registration.ConcreteType.ShouldEqual(typeof(T));
            registration.IsInstance.ShouldBeFalse();
            registration.Dispose.ShouldBeFalse();
            registration.Instance.ShouldBeNull();
        }

        private void Should_be_plugin_instance<T>(Registry.Registration registration, T instance)
        {
            registration.PluginType.ShouldEqual(typeof(IPluginType));
            registration.Singleton.ShouldBeFalse();
            registration.ConcreteType.ShouldEqual(typeof(T));
            registration.IsInstance.ShouldBeTrue();
            registration.Dispose.ShouldBeTrue();
            registration.Instance.ShouldEqual(instance);
        }

        [Test]
        public void Should_get_plugin_instance_from_plugin()
        {
            var instance = new Plugin();

            var plugin = Plugin<IPluginType>.Create(instance);

            plugin.GetInstance(null).ShouldEqual(instance);
        }

        [Test]
        public void Should_get_plugin_type_from_container()
        {
            var container = new Container(x => x
                .For<IPluginType>().Use<Plugin>());
            var plugin = Plugin<IPluginType>.Create<Plugin>();

            var instance = plugin.GetInstance(container);

            instance.ShouldNotBeNull();
            instance.ShouldBeType<Plugin>();
        }
    }
}
