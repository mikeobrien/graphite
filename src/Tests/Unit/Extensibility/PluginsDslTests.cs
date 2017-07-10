using System.Linq;
using Graphite.Extensibility;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Extensibility
{
    [TestFixture]
    public class PluginsDslTests
    {
        public interface IPluginType { }
        public class Plugin1 : IPluginType { }
        public class Plugin2 : IPluginType { }
        public class Plugin3 : IPluginType { }

        private Plugins<IPluginType> _plugins;
        private PluginsDsl<IPluginType> _pluginsDsl;

        [SetUp]
        public void Setup()
        {
            _plugins = new Plugins<IPluginType>(true);
            _pluginsDsl = new PluginsDsl<IPluginType>(_plugins);
        }

        [Test]
        public void Should_clear_plugins()
        {
            _plugins.Append(Plugin<IPluginType>.Create());

            _plugins.Count().ShouldEqual(1);

            _pluginsDsl.Clear();

            _plugins.ShouldBeEmpty();
        }

        [Test]
        public void Should_clear_plugins_except_specified()
        {
            _plugins.Append(Plugin<IPluginType>.Create<Plugin1>());
            _plugins.Append(Plugin<IPluginType>.Create(new Plugin1()));
            _plugins.Append(Plugin<IPluginType>.Create<Plugin2>());

            _plugins.Count().ShouldEqual(3);

            _pluginsDsl.ClearExcept<Plugin2>();

            _plugins.Count().ShouldEqual(1);
            _plugins.First().Type.ShouldEqual(typeof(Plugin2));
        }

        [Test]
        public void Should_removed_specified_type()
        {
            _plugins.Append(Plugin<IPluginType>.Create<Plugin1>());
            _plugins.Append(Plugin<IPluginType>.Create<Plugin2>());
            _plugins.Append(Plugin<IPluginType>.Create(new Plugin2()));

            _plugins.Count().ShouldEqual(3);

            _pluginsDsl.Remove<Plugin2>();

            _plugins.Count().ShouldEqual(1);
            _plugins.First().Type.ShouldEqual(typeof(Plugin1));
        }

        [Test]
        public void Should_replace_specified_type_with_type_plugin(
            [Values(true, false)] bool @default)
        {
            _plugins.Append(Plugin<IPluginType>.Create<Plugin1>());
            _plugins.Append(Plugin<IPluginType>.Create<Plugin2>());
            _plugins.Append(Plugin<IPluginType>.Create(new Plugin2()));

            _pluginsDsl.Replace<Plugin2>().With<Plugin3>(@default);

            _plugins.Count().ShouldEqual(2);
            _plugins.First().Type.ShouldEqual(typeof(Plugin1));

            var replacement = _plugins.Second();
            replacement.Type.ShouldEqual(typeof(Plugin3));
            replacement.Singleton.ShouldBeTrue();

            _plugins.IsDefault(Plugin<IPluginType>.Create<Plugin3>())
                .ShouldEqual(@default);
        }

        [Test]
        public void Should_replace_specified_type_with_instance_plugin(
            [Values(true, false)] bool @default,
            [Values(true, false)] bool dispose)
        {
            var instance3 = new Plugin3();
            _plugins.Append(Plugin<IPluginType>.Create<Plugin1>());
            _plugins.Append(Plugin<IPluginType>.Create<Plugin2>());
            _plugins.Append(Plugin<IPluginType>.Create(new Plugin2()));

            _pluginsDsl.Replace<Plugin2>().With(instance3, dispose, @default);

            _plugins.Count().ShouldEqual(2);
            _plugins.First().Type.ShouldEqual(typeof(Plugin1));

            var replacement = _plugins.Second();
            replacement.Type.ShouldEqual(typeof(Plugin3));
            replacement.Instance.ShouldEqual(instance3);
            replacement.Dispose.ShouldEqual(dispose);

            _plugins.IsDefault(Plugin<IPluginType>.Create(instance3))
                .ShouldEqual(@default);
        }

        [Test]
        public void Should_append_type_plugin(
            [Values(true, false)] bool @default)
        {
            var instance1 = new Plugin1();

            _pluginsDsl.Append(instance1);

            _pluginsDsl.Append<Plugin2>(@default);

            _plugins.Count().ShouldEqual(2);

            _plugins.First().Instance.ShouldEqual(instance1);

            var appended = _plugins.Second();
            appended.Type.ShouldEqual(typeof(Plugin2));
            appended.Singleton.ShouldBeTrue();

            _plugins.IsDefault(Plugin<IPluginType>.Create<Plugin2>())
                .ShouldEqual(@default);
        }

        [Test]
        public void Should_append_instance_plugin(
            [Values(true, false)] bool @default,
            [Values(true, false)] bool dispose)
        {
            var instance2 = new Plugin2();

            _pluginsDsl.Append<Plugin1>();
            _pluginsDsl.Append(instance2, dispose, @default);

            _plugins.Count().ShouldEqual(2);

            _plugins.First().Type.ShouldEqual(typeof(Plugin1));

            var appended = _plugins.Second();
            appended.Type.ShouldEqual(typeof(Plugin2));
            appended.Instance.ShouldEqual(instance2);
            appended.Dispose.ShouldEqual(dispose);

            _plugins.IsDefault(Plugin<IPluginType>.Create(instance2))
                .ShouldEqual(@default);
        }

        [Test]
        public void Should_append_type_plugin_after_another(
            [Values(true, false)] bool @default,
            [Values(true, false)] bool orAppend)
        {
            var instance1 = new Plugin1();

            _pluginsDsl.Append(instance1);
            _pluginsDsl.Append<Plugin2>();

            var dsl = _pluginsDsl.Append<Plugin3>(@default);
            
            if (orAppend) dsl.AfterOrAppend<Plugin1>();
            else dsl.AfterOrPrepend<Plugin1>();

            _plugins.Count().ShouldEqual(3);

            _plugins.First().Type.ShouldEqual(typeof(Plugin1));

            var appended = _plugins.Second();
            appended.Type.ShouldEqual(typeof(Plugin3));
            appended.Singleton.ShouldBeTrue();

            _plugins.Third().Type.ShouldEqual(typeof(Plugin2));

            _plugins.IsDefault(Plugin<IPluginType>.Create<Plugin3>())
                .ShouldEqual(@default);
        }

        [Test]
        public void Should_append_instance_plugin_after_another(
            [Values(true, false)] bool @default,
            [Values(true, false)] bool dispose,
            [Values(true, false)] bool orAppend)
        {
            var instance3 = new Plugin3();

            _pluginsDsl.Append<Plugin1>();
            _pluginsDsl.Append<Plugin2>();

            var dsl = _pluginsDsl.Append(instance3, dispose, @default);

            if (orAppend) dsl.AfterOrAppend<Plugin1>();
            else dsl.AfterOrPrepend<Plugin1>();

            _plugins.Count().ShouldEqual(3);

            _plugins.First().Type.ShouldEqual(typeof(Plugin1));

            var appended = _plugins.Second();
            appended.Type.ShouldEqual(typeof(Plugin3));
            appended.Instance.ShouldEqual(instance3);
            appended.Dispose.ShouldEqual(dispose);

            _plugins.Third().Type.ShouldEqual(typeof(Plugin2));

            _plugins.IsDefault(Plugin<IPluginType>.Create(instance3))
                .ShouldEqual(@default);
        }

        [Test]
        public void Should_append_type_plugin_if_it_cannot_append_after_another(
            [Values(true, false)] bool @default)
        {
            var instance1 = new Plugin1();

            _pluginsDsl.Append(instance1);

            _pluginsDsl.Append<Plugin2>(@default).AfterOrAppend<Plugin3>();

            _plugins.Count().ShouldEqual(2);

            _plugins.First().Instance.ShouldEqual(instance1);

            var appended = _plugins.Second();
            appended.Type.ShouldEqual(typeof(Plugin2));
            appended.Singleton.ShouldBeTrue();

            _plugins.IsDefault(Plugin<IPluginType>.Create<Plugin2>())
                .ShouldEqual(@default);
        }

        [Test]
        public void Should_append_instance_plugin_if_it_cannot_append_after_another(
            [Values(true, false)] bool @default,
            [Values(true, false)] bool dispose)
        {
            var instance2 = new Plugin2();

            _pluginsDsl.Append<Plugin1>();
            _pluginsDsl.Append(instance2, dispose, @default).AfterOrAppend<Plugin3>();

            _plugins.Count().ShouldEqual(2);

            _plugins.First().Type.ShouldEqual(typeof(Plugin1));

            var appended = _plugins.Second();
            appended.Type.ShouldEqual(typeof(Plugin2));
            appended.Instance.ShouldEqual(instance2);
            appended.Dispose.ShouldEqual(dispose);

            _plugins.IsDefault(Plugin<IPluginType>.Create(instance2))
                .ShouldEqual(@default);
        }

        [Test]
        public void Should_prepend_type_plugin_if_it_cannot_append_after_another(
            [Values(true, false)] bool @default)
        {
            var instance1 = new Plugin1();

            _pluginsDsl.Append(instance1);

            _pluginsDsl.Append<Plugin2>(@default).AfterOrPrepend<Plugin3>();

            _plugins.Count().ShouldEqual(2);
            
            var prepended = _plugins.First();
            prepended.Type.ShouldEqual(typeof(Plugin2));
            prepended.Singleton.ShouldBeTrue();

            _plugins.Second().Instance.ShouldEqual(instance1);

            _plugins.IsDefault(Plugin<IPluginType>.Create<Plugin2>())
                .ShouldEqual(@default);
        }

        [Test]
        public void Should_prepend_instance_plugin_if_it_cannot_append_after_another(
            [Values(true, false)] bool @default,
            [Values(true, false)] bool dispose)
        {
            var instance2 = new Plugin2();

            _pluginsDsl.Append<Plugin1>();
            _pluginsDsl.Append(instance2, dispose, @default).AfterOrPrepend<Plugin3>();

            _plugins.Count().ShouldEqual(2);

            var prepended = _plugins.First();
            prepended.Type.ShouldEqual(typeof(Plugin2));
            prepended.Instance.ShouldEqual(instance2);
            prepended.Dispose.ShouldEqual(dispose);

            _plugins.Second().Type.ShouldEqual(typeof(Plugin1));

            _plugins.IsDefault(Plugin<IPluginType>.Create(instance2))
                .ShouldEqual(@default);
        }
        
        [Test]
        public void Should_prepend_type_plugin(
            [Values(true, false)] bool @default)
        {
            var instance1 = new Plugin1();

            _pluginsDsl.Append(instance1);
            _pluginsDsl.Prepend<Plugin2>(@default);

            _plugins.Count().ShouldEqual(2);
            
            var prepended = _plugins.First();
            prepended.Type.ShouldEqual(typeof(Plugin2));
            prepended.Singleton.ShouldBeTrue();

            _plugins.Second().Instance.ShouldEqual(instance1);

            _plugins.IsDefault(Plugin<IPluginType>.Create<Plugin2>())
                .ShouldEqual(@default);
        }

        [Test]
        public void Should_prepend_instance_plugin(
            [Values(true, false)] bool @default,
            [Values(true, false)] bool dispose)
        {
            var instance2 = new Plugin2();

            _pluginsDsl.Append<Plugin1>();
            _pluginsDsl.Prepend(instance2, dispose, @default);

            _plugins.Count().ShouldEqual(2);

            var prepended = _plugins.First();
            prepended.Type.ShouldEqual(typeof(Plugin2));
            prepended.Instance.ShouldEqual(instance2);
            prepended.Dispose.ShouldEqual(dispose);

            _plugins.Second().Type.ShouldEqual(typeof(Plugin1));

            _plugins.IsDefault(Plugin<IPluginType>.Create(instance2))
                .ShouldEqual(@default);
        }

        [Test]
        public void Should_prepend_type_plugin_before_another(
            [Values(true, false)] bool @default,
            [Values(true, false)] bool orPrepend)
        {
            var instance1 = new Plugin1();

            _pluginsDsl.Append(instance1);
            _pluginsDsl.Append<Plugin2>();

            var dsl = _pluginsDsl.Prepend<Plugin3>(@default);

            if (orPrepend) dsl.BeforeOrPrepend<Plugin2>();
            else dsl.BeforeOrAppend<Plugin2>();

            _plugins.Count().ShouldEqual(3);

            _plugins.First().Instance.ShouldEqual(instance1);

            var prepended = _plugins.Second();
            prepended.Type.ShouldEqual(typeof(Plugin3));
            prepended.Singleton.ShouldBeTrue();

            _plugins.Third().Type.ShouldEqual(typeof(Plugin2));

            _plugins.IsDefault(Plugin<IPluginType>.Create<Plugin3>())
                .ShouldEqual(@default);
        }

        [Test]
        public void Should_prepend_instance_plugin_before_another(
            [Values(true, false)] bool @default,
            [Values(true, false)] bool dispose,
            [Values(true, false)] bool orPrepend)
        {
            var instance3 = new Plugin3();

            _pluginsDsl.Append<Plugin1>();
            _pluginsDsl.Append<Plugin2>();

            var dsl = _pluginsDsl.Prepend(instance3, dispose, @default);

            if (orPrepend) dsl.BeforeOrPrepend<Plugin2>();
            else dsl.BeforeOrAppend<Plugin2>();

            _plugins.Count().ShouldEqual(3);

            _plugins.First().Type.ShouldEqual(typeof(Plugin1));

            var appended = _plugins.Second();
            appended.Type.ShouldEqual(typeof(Plugin3));
            appended.Instance.ShouldEqual(instance3);
            appended.Dispose.ShouldEqual(dispose);

            _plugins.Third().Type.ShouldEqual(typeof(Plugin2));

            _plugins.IsDefault(Plugin<IPluginType>.Create(instance3))
                .ShouldEqual(@default);
        }

        [Test]
        public void Should_append_type_plugin_if_it_cannot_prepend_before_another(
            [Values(true, false)] bool @default)
        {
            var instance1 = new Plugin1();

            _pluginsDsl.Append(instance1);

            _pluginsDsl.Prepend<Plugin2>(@default).BeforeOrAppend<Plugin3>();

            _plugins.Count().ShouldEqual(2);

            _plugins.First().Instance.ShouldEqual(instance1);

            var appended = _plugins.Second();
            appended.Type.ShouldEqual(typeof(Plugin2));
            appended.Singleton.ShouldBeTrue();

            _plugins.IsDefault(Plugin<IPluginType>.Create<Plugin2>())
                .ShouldEqual(@default);
        }

        [Test]
        public void Should_append_instance_plugin_if_it_cannot_prepend_before_another(
            [Values(true, false)] bool @default,
            [Values(true, false)] bool dispose)
        {
            var instance2 = new Plugin2();

            _pluginsDsl.Append<Plugin1>();
            _pluginsDsl.Prepend(instance2, dispose, @default).BeforeOrAppend<Plugin3>();

            _plugins.Count().ShouldEqual(2);

            _plugins.First().Type.ShouldEqual(typeof(Plugin1));

            var appended = _plugins.Second();
            appended.Type.ShouldEqual(typeof(Plugin2));
            appended.Instance.ShouldEqual(instance2);
            appended.Dispose.ShouldEqual(dispose);

            _plugins.IsDefault(Plugin<IPluginType>.Create(instance2))
                .ShouldEqual(@default);
        }

        [Test]
        public void Should_prepend_type_plugin_if_it_cannot_prepend_before_another(
            [Values(true, false)] bool @default)
        {
            var instance1 = new Plugin1();

            _pluginsDsl.Append(instance1);

            _pluginsDsl.Prepend<Plugin2>(@default).BeforeOrPrepend<Plugin3>();

            _plugins.Count().ShouldEqual(2);
            
            var prepended = _plugins.First();
            prepended.Type.ShouldEqual(typeof(Plugin2));
            prepended.Singleton.ShouldBeTrue();

            _plugins.Second().Instance.ShouldEqual(instance1);

            _plugins.IsDefault(Plugin<IPluginType>.Create<Plugin2>())
                .ShouldEqual(@default);
        }

        [Test]
        public void Should_prepend_instance_plugin_if_it_cannot_prepend_before_another(
            [Values(true, false)] bool @default,
            [Values(true, false)] bool dispose)
        {
            var instance2 = new Plugin2();

            _pluginsDsl.Append<Plugin1>();
            _pluginsDsl.Prepend(instance2, dispose, @default).BeforeOrPrepend<Plugin3>();

            _plugins.Count().ShouldEqual(2);

            var prepended = _plugins.First();
            prepended.Type.ShouldEqual(typeof(Plugin2));
            prepended.Instance.ShouldEqual(instance2);
            prepended.Dispose.ShouldEqual(dispose);

            _plugins.Second().Type.ShouldEqual(typeof(Plugin1));

            _plugins.IsDefault(Plugin<IPluginType>.Create(instance2))
                .ShouldEqual(@default);
        }
    }
}
