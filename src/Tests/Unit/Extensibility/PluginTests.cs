using Graphite.Extensibility;
using NUnit.Framework;
using Should;

namespace Tests.Unit.Extensibility
{
    [TestFixture]
    public class PluginTests
    {
        public interface IPluginType { }
        public class Plugin1 : IPluginType { }
        public class Plugin2 : IPluginType { }

        [Test]
        public void Should_set_instance()
        {
            var plugin = Plugin<IPluginType>.Create<Plugin1>(true);

            plugin.Singleton.ShouldBeTrue();
            plugin.HasInstance.ShouldBeFalse();
            plugin.Type.ShouldEqual(typeof(Plugin1));
            plugin.Instance.ShouldBeNull();
            plugin.Dispose.ShouldBeFalse();

            var instance = new Plugin2();

            plugin.Set(instance, true);

            plugin.Singleton.ShouldBeFalse();
            plugin.HasInstance.ShouldBeTrue();
            plugin.Type.ShouldEqual(typeof(Plugin2));
            plugin.Instance.ShouldEqual(instance);
            plugin.Dispose.ShouldBeTrue();
        }

        [Test]
        public void Should_set_type()
        {
            var instance = new Plugin2();
            var plugin = Plugin<IPluginType>.Create(instance, true);

            plugin.Singleton.ShouldBeFalse();
            plugin.HasInstance.ShouldBeTrue();
            plugin.Type.ShouldEqual(typeof(Plugin2));
            plugin.Instance.ShouldEqual(instance);
            plugin.Dispose.ShouldBeTrue();

            plugin.Set<Plugin1>(true);

            plugin.Singleton.ShouldBeTrue();
            plugin.HasInstance.ShouldBeFalse();
            plugin.Type.ShouldEqual(typeof(Plugin1));
            plugin.Instance.ShouldBeNull();
            plugin.Dispose.ShouldBeFalse();
        }

        [Test]
        public void Should_clone_instance_plugin()
        {
            var instance = new Plugin1();
            var plugin = Plugin<IPluginType>.Create(instance, true);

            var clone = plugin.Clone();

            (clone == plugin).ShouldBeFalse();
            clone.ShouldEqual(plugin);

            clone.Singleton.ShouldEqual(plugin.Singleton);
            clone.HasInstance.ShouldEqual(plugin.HasInstance);
            clone.Type.ShouldEqual(plugin.Type);
            clone.Instance.ShouldEqual(plugin.Instance);
            clone.Dispose.ShouldEqual(plugin.Dispose);
        }

        [Test]
        public void Should_clone_type_plugin()
        {
            var plugin = Plugin<IPluginType>.Create<Plugin1>(true);

            var clone = plugin.Clone();

            (clone == plugin).ShouldBeFalse();
            clone.ShouldEqual(plugin);

            clone.Singleton.ShouldEqual(plugin.Singleton);
            clone.HasInstance.ShouldEqual(plugin.HasInstance);
            clone.Type.ShouldEqual(plugin.Type);
            clone.Instance.ShouldEqual(plugin.Instance);
            clone.Dispose.ShouldEqual(plugin.Dispose);
        }

        [Test]
        public void Should_generate_instance_hash_code()
        {
            var instance = new Plugin1();
            var plugin1a = Plugin<IPluginType>.Create(instance);
            var plugin1b = Plugin<IPluginType>.Create(instance);
            var plugin2 = Plugin<IPluginType>.Create(new Plugin1());

            plugin1a.GetHashCode().ShouldNotEqual(instance.GetHashCode());
            plugin1a.GetHashCode().ShouldNotEqual(instance.GetType().GetHashCode());

            plugin1a.GetHashCode().ShouldEqual(plugin1b.GetHashCode());
            plugin1a.GetHashCode().ShouldNotEqual(plugin2.GetHashCode());
        }

        [Test]
        public void Should_generate_type_hash_code()
        {
            var plugin1a = Plugin<IPluginType>.Create<Plugin1>();
            var plugin1b = Plugin<IPluginType>.Create<Plugin1>();
            var plugin2 = Plugin<IPluginType>.Create<Plugin2>();
            
            plugin1a.GetHashCode().ShouldNotEqual(typeof(Plugin1).GetHashCode());

            plugin1a.GetHashCode().ShouldEqual(plugin1b.GetHashCode());
            plugin1a.GetHashCode().ShouldNotEqual(plugin2.GetHashCode());
        }

        [Test]
        public void Should_indicate_equality()
        {
            var plugin1a = Plugin<IPluginType>.Create<Plugin1>();
            var plugin1b = Plugin<IPluginType>.Create<Plugin1>();
            var plugin2 = Plugin<IPluginType>.Create<Plugin2>();

            plugin1a.ShouldNotEqual(null);
            plugin1a.ShouldNotEqual(new object());
            plugin1a.ShouldEqual(plugin1a);
            plugin1a.ShouldEqual(plugin1b);
            plugin1a.ShouldNotEqual(plugin2);
        }
    }
}
