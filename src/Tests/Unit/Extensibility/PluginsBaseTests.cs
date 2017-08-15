using System;
using System.Linq;
using Graphite.Extensibility;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Extensibility
{
    [TestFixture]
    public class PluginsBaseTests
    {
        public interface IPluginType { }
        public class Plugin1 : IPluginType { }
        public class Plugin2 : IPluginType { }
        public class Plugin3 : IPluginType { }
        public class Plugin4 : IPluginType { }
        public class Context { }

        private Plugins<IPluginType> _plugins;

        [SetUp]
        public void Setup()
        {
            _plugins = new Plugins<IPluginType>(true);
        }

        [Test]
        public void Should_indicate_if_singleton([Values(true, false)] bool singleton)
        {
            new Plugins<IPluginType>(singleton).Singleton.ShouldEqual(singleton);
        }
        
        [Test]
        public void Should_return_index_of_first_plugin_instance_if_instance_exists()
        {
            var instance = new Plugin1();

            _plugins.Append(Plugin<IPluginType>.Create<Plugin1>());
            _plugins.Append(Plugin<IPluginType>.Create(new Plugin1()));
            _plugins.Append(Plugin<IPluginType>.Create(instance));
            _plugins.Append(Plugin<IPluginType>.Create(instance));

            _plugins.IndexOf(instance).ShouldEqual(2);
        }

        [Test]
        public void Should_return_index_of_first_plugin_type_if_instance_does_not_exist()
        {
            _plugins.Append(Plugin<IPluginType>.Create(new Plugin1()));
            _plugins.Append(Plugin<IPluginType>.Create<Plugin1>());
            _plugins.Append(Plugin<IPluginType>.Create<Plugin1>());

            _plugins.IndexOf(new Plugin1()).ShouldEqual(1);
        }

        [Test]
        public void Should_return_max_index_if_instance_or_type_does_not_exist()
        {
            _plugins.Append(Plugin<IPluginType>.Create(new Plugin1()));
            _plugins.Append(Plugin<IPluginType>.Create<Plugin2>());

            _plugins.IndexOf(new Plugin1()).ShouldEqual(short.MaxValue);
        }

        [Test]
        public void Should_return_index_of_plugin()
        {
            var plugin1 = Plugin<IPluginType>.Create<Plugin1>();
            var plugin2 = Plugin<IPluginType>.Create(new Plugin1());

            _plugins.Append(plugin1);
            _plugins.Append(plugin2);

            _plugins.IndexOf(plugin1).ShouldEqual(0);
            _plugins.IndexOf(plugin2).ShouldEqual(1);
        }

        [Test]
        public void Should_set_default_plugin()
        {
            var plugin1 = Plugin<IPluginType>.Create<Plugin1>();

            _plugins.DefaultIs(plugin1);

            _plugins.IsDefault(plugin1).ShouldBeTrue();
        }

        [Test]
        public void Should_indicate_if_type_plugin_is_default()
        {
            var plugin1 = Plugin<IPluginType>.Create<Plugin1>();

            _plugins.DefaultIs(plugin1);

            _plugins.IsDefault(plugin1).ShouldBeTrue();
            _plugins.IsDefault(Plugin<IPluginType>.Create<Plugin1>()).ShouldBeTrue();
            _plugins.IsDefault(Plugin<IPluginType>.Create(new Plugin1())).ShouldBeFalse();
        }

        [Test]
        public void Should_indicate_if_instance_plugin_is_default()
        {
            var plugin1 = Plugin<IPluginType>.Create(new Plugin1());

            _plugins.DefaultIs(plugin1);

            _plugins.IsDefault(plugin1).ShouldBeTrue();
            _plugins.IsDefault(Plugin<IPluginType>.Create<Plugin1>()).ShouldBeFalse();
            _plugins.IsDefault(Plugin<IPluginType>.Create(new Plugin1())).ShouldBeFalse();
        }

        [Test]
        public void Should_get_the_default_instance()
        {
            var plugin1 = new Plugin1();
            var plugin2 = new Plugin2();
            var plugin3 = new Plugin3();
            var plugin4 = new Plugin4();

            var instances = new IPluginType[] { plugin1, plugin2, plugin3, plugin4 };

            var plugins = new ConditionalPlugins<IPluginType, Context>(false)
                .Configure(x => x
                    .Append<Plugin1>()
                    .Append<Plugin2>(@default: true)
                    .Append<Plugin3>(@default: true)
                    .Append<Plugin4>());

            plugins.GetDefaultInstance(instances).ShouldEqual(plugin3);
        }

        [Test]
        public void Should_return_first_instance_plugin_by_instance()
        {
            var instance = new Plugin1();
            var plugin = Plugin<IPluginType>.Create(instance);

            _plugins.Append(Plugin<IPluginType>.Create<Plugin1>());
            _plugins.Append(Plugin<IPluginType>.Create(new Plugin1()));
            _plugins.Append(plugin);
            _plugins.Append(Plugin<IPluginType>.Create(instance));

            var result = _plugins.InstancePluginFor(instance);
            
            result.ShouldEqual(plugin);
        }

        [Test]
        public void Should_return_type_plugin_by_type()
        {
            var plugin = Plugin<IPluginType>.Create<Plugin1>();

            _plugins.Append(Plugin<IPluginType>.Create(new Plugin1()));
            _plugins.Append(plugin);

            var result = _plugins.TypePluginFor(typeof(Plugin1));

            result.ShouldEqual(plugin);
        }

        [Test]
        public void Should_return_type_plugin_by_generic_type()
        {
            var plugin = Plugin<IPluginType>.Create<Plugin1>();

            _plugins.Append(Plugin<IPluginType>.Create(new Plugin1()));
            _plugins.Append(plugin);

            var result = _plugins.TypePluginFor<Plugin1>();

            result.ShouldEqual(plugin);
        }

        [Test]
        public void Should_return_first_instance_plugin_by_instance_if_exists()
        {
            var instance = new Plugin1();
            var plugin = Plugin<IPluginType>.Create(instance);

            _plugins.Append(Plugin<IPluginType>.Create<Plugin1>());
            _plugins.Append(Plugin<IPluginType>.Create(new Plugin1()));
            _plugins.Append(plugin);
            _plugins.Append(Plugin<IPluginType>.Create(instance));

            var result = _plugins.InstanceOrTypePluginFor(instance);

            result.ShouldEqual(plugin);
        }

        [Test]
        public void Should_return_type_plugin_by_instance_if_instance_does_not_exist()
        {
            var plugin = Plugin<IPluginType>.Create<Plugin1>();

            _plugins.Append(Plugin<IPluginType>.Create(new Plugin1()));
            _plugins.Append(plugin);

            var result = _plugins.InstanceOrTypePluginFor(new Plugin1());

            result.ShouldEqual(plugin);
        }

        [Test]
        public void Should_return_plugins_for_instances(
            [Values(true, false)] bool @default)
        {
            var instance1 = new Plugin1();
            var instance2 = new Plugin1();
            var instance3 = new Plugin1();

            var plugin2 = Plugin<IPluginType>.Create(instance2);
            var plugin3 = Plugin<IPluginType>.Create(instance3);
            var typePlugin = Plugin<IPluginType>.Create<Plugin1>();
            
            _plugins.Append(Plugin<IPluginType>.Create(new Plugin1()));
            _plugins.Append(plugin2, @default);
            _plugins.Append(plugin3);
            _plugins.Append(typePlugin);

            var result = _plugins.PluginsFor(new [] { instance3, instance1, instance2 }).ToList();

            result.Count.ShouldEqual(3);

            var plugin = result.First();

            plugin.Instance.ShouldEqual(instance2);
            plugin.IsDefault.ShouldEqual(@default);
            plugin.Plugin.ShouldEqual(plugin2);

            plugin = result.Second();

            plugin.Instance.ShouldEqual(instance3);
            plugin.IsDefault.ShouldBeFalse();
            plugin.Plugin.ShouldEqual(plugin3);

            plugin = result.Third();

            plugin.Instance.ShouldEqual(instance1);
            plugin.IsDefault.ShouldBeFalse();
            plugin.Plugin.ShouldEqual(typePlugin);
        }

        [Test]
        public void Should_not_return_plugins_for_instances_that_dont_map()
        {
            var instance1 = new Plugin1();
            var instance2 = new Plugin1();
            var instance3 = new Plugin1();

            var plugin2 = Plugin<IPluginType>.Create(instance2);
            var plugin3 = Plugin<IPluginType>.Create(instance3);
            
            _plugins.Append(plugin2);
            _plugins.Append(plugin3);

            var result = _plugins.PluginsFor(new[] { instance3, instance1, instance2 }).ToList();

            result.Count.ShouldEqual(2);

            var plugin = result.First();

            plugin.Instance.ShouldEqual(instance2);
            plugin.Plugin.ShouldEqual(plugin2);

            plugin = result.Second();

            plugin.Instance.ShouldEqual(instance3);
            plugin.Plugin.ShouldEqual(plugin3);
        }

        [Test]
        public void Should_return_all_plugins_of_the_specified_type()
        {
            var plugin1 = Plugin<IPluginType>.Create(new Plugin1());
            var plugin2 = Plugin<IPluginType>.Create(new Plugin1());
            var plugin3 = Plugin<IPluginType>.Create<Plugin1>();

            _plugins.Append(Plugin<IPluginType>.Create(new Plugin2()));
            _plugins.Append(plugin1);
            _plugins.Append(plugin2);
            _plugins.Append(Plugin<IPluginType>.Create<Plugin3>());
            _plugins.Append(plugin3);

            _plugins.AllOfType(typeof(Plugin1)).ShouldOnlyContain(plugin1, plugin2, plugin3);
        }

        [Test]
        public void Should_return_all_plugins_of_the_specified_generic_type()
        {
            var plugin1 = Plugin<IPluginType>.Create(new Plugin1());
            var plugin2 = Plugin<IPluginType>.Create(new Plugin1());
            var plugin3 = Plugin<IPluginType>.Create<Plugin1>();

            _plugins.Append(Plugin<IPluginType>.Create(new Plugin2()));
            _plugins.Append(plugin1);
            _plugins.Append(plugin2);
            _plugins.Append(Plugin<IPluginType>.Create<Plugin3>());
            _plugins.Append(plugin3);

            _plugins.AllOfType<Plugin1>().ShouldOnlyContain(plugin1, plugin2, plugin3);
        }

        [Test]
        public void Should_return_first_plugin_of_the_specified_type()
        {
            var plugin1 = Plugin<IPluginType>.Create(new Plugin1());
            var plugin2 = Plugin<IPluginType>.Create(new Plugin1());
            var plugin3 = Plugin<IPluginType>.Create<Plugin1>();

            _plugins.Append(Plugin<IPluginType>.Create(new Plugin2()));
            _plugins.Append(plugin1);
            _plugins.Append(plugin2);
            _plugins.Append(Plugin<IPluginType>.Create<Plugin3>());
            _plugins.Append(plugin3);

            _plugins.FirstOfType(typeof(Plugin1)).ShouldEqual(plugin1);
        }

        [Test]
        public void Should_return_first_plugin_of_the_specified_generic_type()
        {
            var plugin1 = Plugin<IPluginType>.Create(new Plugin1());
            var plugin2 = Plugin<IPluginType>.Create(new Plugin1());
            var plugin3 = Plugin<IPluginType>.Create<Plugin1>();

            _plugins.Append(Plugin<IPluginType>.Create(new Plugin2()));
            _plugins.Append(plugin1);
            _plugins.Append(plugin2);
            _plugins.Append(Plugin<IPluginType>.Create<Plugin3>());
            _plugins.Append(plugin3);

            _plugins.FirstOfType<Plugin1>().ShouldEqual(plugin1);
        }

        [Test]
        public void Should_return_last_plugin_of_the_specified_type()
        {
            var plugin1 = Plugin<IPluginType>.Create(new Plugin1());
            var plugin2 = Plugin<IPluginType>.Create(new Plugin1());
            var plugin3 = Plugin<IPluginType>.Create<Plugin1>();

            _plugins.Append(Plugin<IPluginType>.Create(new Plugin2()));
            _plugins.Append(plugin1);
            _plugins.Append(plugin2);
            _plugins.Append(Plugin<IPluginType>.Create<Plugin3>());
            _plugins.Append(plugin3);

            _plugins.LastOfType(typeof(Plugin1)).ShouldEqual(plugin3);
        }

        [Test]
        public void Should_return_last_plugin_of_the_specified_generic_type()
        {
            var plugin1 = Plugin<IPluginType>.Create(new Plugin1());
            var plugin2 = Plugin<IPluginType>.Create(new Plugin1());
            var plugin3 = Plugin<IPluginType>.Create<Plugin1>();

            _plugins.Append(Plugin<IPluginType>.Create(new Plugin2()));
            _plugins.Append(plugin1);
            _plugins.Append(plugin2);
            _plugins.Append(Plugin<IPluginType>.Create<Plugin3>());
            _plugins.Append(plugin3);

            _plugins.LastOfType<Plugin1>().ShouldEqual(plugin3);
        }

        [Test]
        public void Should_clear_plugins()
        {
            _plugins.Append(Plugin<IPluginType>.Create());

            _plugins.Count().ShouldEqual(1);

            _plugins.Clear();

            _plugins.ShouldBeEmpty();
        }

        [Test]
        public void Should_clear_all_plugins_except_specified_type()
        {
            _plugins.Append(Plugin<IPluginType>.Create<Plugin1>());
            _plugins.Append(Plugin<IPluginType>.Create(new Plugin1()));
            _plugins.Append(Plugin<IPluginType>.Create<Plugin2>());

            _plugins.Count().ShouldEqual(3);

            _plugins.ClearExcept<Plugin2>();

            _plugins.Count().ShouldEqual(1);
            _plugins.First().Type.ShouldEqual(typeof(Plugin2));
        }

        [Test]
        public void Should_indicate_if_contains_any_of_type(
            [Values(true, false)] bool any,
            [Values(true, false)] bool instance)
        {
            _plugins.Append(Plugin<IPluginType>.Create<Plugin1>());

            if (any)
                _plugins.Append(instance
                    ? Plugin<IPluginType>.Create(new Plugin2())
                    : Plugin<IPluginType>.Create<Plugin2>());
            
            _plugins.AnyOfType(typeof(Plugin2)).ShouldEqual(any);
        }

        [Test]
        public void Should_indicate_if_contains_any_of_generic_type(
            [Values(true, false)] bool any,
            [Values(true, false)] bool instance)
        {
            _plugins.Append(Plugin<IPluginType>.Create<Plugin1>());

            if (any)
                _plugins.Append(instance
                    ? Plugin<IPluginType>.Create(new Plugin2())
                    : Plugin<IPluginType>.Create<Plugin2>());

            _plugins.AnyOfType<Plugin2>().ShouldEqual(any);
        }

        [Test]
        public void Should_remove_list_of_plugins()
        {
            var keep1 = Plugin<IPluginType>.Create(new Plugin1());
            var keep2 = Plugin<IPluginType>.Create<Plugin2>();

            var remove1 = Plugin<IPluginType>.Create(new Plugin1());
            var remove2 = Plugin<IPluginType>.Create(new Plugin1());
            var remove3 = Plugin<IPluginType>.Create<Plugin1>();

            _plugins.Append(keep1);
            _plugins.Append(remove1);
            _plugins.Append(remove2);
            _plugins.Append(keep2);
            _plugins.Append(remove3);

            _plugins.Remove(new [] { remove1, remove2, remove3, remove3 })
                .ShouldOnlyContain(keep1, keep2);
        }

        [Test]
        public void Should_remove_instance_plugin()
        {
            var keep1 = Plugin<IPluginType>.Create(new Plugin1());
            var keep2 = Plugin<IPluginType>.Create<Plugin1>();

            var remove = Plugin<IPluginType>.Create(new Plugin1());

            _plugins.Append(keep1);
            _plugins.Append(remove);
            _plugins.Append(keep2);

            _plugins.Remove(remove)
                .ShouldOnlyContain(keep1, keep2);
        }

        [Test]
        public void Should_remove_type_plugin()
        {
            var keep1 = Plugin<IPluginType>.Create(new Plugin1());
            var keep2 = Plugin<IPluginType>.Create(new Plugin1());

            var remove = Plugin<IPluginType>.Create<Plugin1>();

            _plugins.Append(keep1);
            _plugins.Append(remove);
            _plugins.Append(keep2);

            _plugins.Remove(remove)
                .ShouldOnlyContain(keep1, keep2);
        }

        [Test]
        public void Should_remove_type_plugin_by_type()
        {
            var keep1 = Plugin<IPluginType>.Create(new Plugin1());
            var keep2 = Plugin<IPluginType>.Create(new Plugin1());

            var remove = Plugin<IPluginType>.Create<Plugin1>();

            _plugins.Append(keep1);
            _plugins.Append(remove);
            _plugins.Append(keep2);

            _plugins.RemoveTypePlugin<Plugin1>()
                .ShouldOnlyContain(keep1, keep2);
        }

        [Test]
        public void Should_remove_instance_plugin_by_instance()
        {
            var keep1 = Plugin<IPluginType>.Create(new Plugin1());
            var keep2 = Plugin<IPluginType>.Create<Plugin1>();

            var instance = new Plugin1();
            var remove = Plugin<IPluginType>.Create(instance);

            _plugins.Append(keep1);
            _plugins.Append(remove);
            _plugins.Append(keep2);

            _plugins.RemoveInstancePlugin(instance)
                .ShouldOnlyContain(keep1, keep2);
        }

        [Test]
        public void Should_remove_plugins_of_the_specified_type()
        {
            var keep1 = Plugin<IPluginType>.Create(new Plugin2());
            var keep2 = Plugin<IPluginType>.Create<Plugin3>();

            _plugins.Append(Plugin<IPluginType>.Create<Plugin1>());
            _plugins.Append(keep1);
            _plugins.Append(Plugin<IPluginType>.Create(new Plugin1()));
            _plugins.Append(Plugin<IPluginType>.Create(new Plugin1()));
            _plugins.Append(keep2);

            _plugins.RemoveAllOfType<Plugin1>()
                .ShouldOnlyContain(keep1, keep2);
        }

        [Test]
        public void Should_replace_all_type_plugins_of_the_specified_type(
            [Values(true, false)] bool @default)
        {
            var keep1 = Plugin<IPluginType>.Create(new Plugin2());
            var keep2 = Plugin<IPluginType>.Create(new Plugin1());
            var keep3 = Plugin<IPluginType>.Create<Plugin3>();

            var replace = Plugin<IPluginType>.Create(new Plugin3());

            _plugins.Append(keep1);
            _plugins.Append(Plugin<IPluginType>.Create<Plugin1>());
            _plugins.Append(keep2);
            _plugins.Append(keep3);

            _plugins.ReplaceTypePluginWith<Plugin1>(replace, @default)
                .ShouldOnlyContain(keep1, replace, keep2, keep3);

            _plugins.IsDefault(replace).ShouldEqual(@default);
        }

        [Test]
        public void Should_replace_all_instance_plugins_of_the_specified_instance(
            [Values(true, false)] bool @default)
        {
            var keep1 = Plugin<IPluginType>.Create(new Plugin2());
            var keep2 = Plugin<IPluginType>.Create<Plugin1>();
            var keep3 = Plugin<IPluginType>.Create(new Plugin1());

            var instance = new Plugin4();
            var replaceWith = Plugin<IPluginType>.Create(new Plugin3());

            _plugins.Append(keep1);
            _plugins.Append(keep2);
            _plugins.Append(Plugin<IPluginType>.Create(instance));
            _plugins.Append(keep3);

            _plugins.ReplaceInstancePluginWith(instance, replaceWith, @default)
                .ShouldOnlyContain(keep1, keep2, replaceWith, keep3);

            _plugins.IsDefault(replaceWith).ShouldEqual(@default);
        }

        [Test]
        public void Should_replace_all_of_the_specified_type_with(
            [Values(true, false)] bool @default)
        {
            var keep1 = Plugin<IPluginType>.Create(new Plugin2());
            var keep2 = Plugin<IPluginType>.Create<Plugin2>();
            
            var replaceWith = Plugin<IPluginType>.Create(new Plugin3());

            _plugins.Append(keep1);
            _plugins.Append(Plugin<IPluginType>.Create<Plugin1>());
            _plugins.Append(Plugin<IPluginType>.Create(new Plugin1()));
            _plugins.Append(keep2);
            _plugins.Append(Plugin<IPluginType>.Create(new Plugin1()));

            _plugins.ReplaceAllOfTypeWith<Plugin1>(replaceWith, @default)
                .ShouldOnlyContain(keep1, keep2, replaceWith);

            _plugins.IsDefault(replaceWith).ShouldEqual(@default);
        }

        [Test]
        public void Should_replace_plugin(
            [Values(true, false)] bool @default)
        {
            var keep1 = Plugin<IPluginType>.Create(new Plugin2());
            var keep2 = Plugin<IPluginType>.Create<Plugin2>();

            var replace = Plugin<IPluginType>.Create<Plugin1>();
            var replaceWith = Plugin<IPluginType>.Create(new Plugin3());

            _plugins.Append(keep1);
            _plugins.Append(replace);
            _plugins.Append(keep2);

            _plugins.ReplaceWith(replace, replaceWith, @default)
                .ShouldOnlyContain(keep1, replaceWith, keep2);

            _plugins.IsDefault(replaceWith).ShouldEqual(@default);
        }

        [Test]
        public void Should_append_plugin(
            [Values(true, false)] bool @default)
        {
            var plugin1 = Plugin<IPluginType>.Create<Plugin1>();
            var append = Plugin<IPluginType>.Create<Plugin2>();

            _plugins.Append(plugin1);

            _plugins.Append(append, @default)
                .ShouldOnlyContain(plugin1, append);

            _plugins.IsDefault(append).ShouldEqual(@default);
        }

        [Test]
        public void Should_append_plugin_after_last_of_type_when_it_exists_and_not_append(
            [Values(true, false)] bool @default)
        {
            var plugin1a = Plugin<IPluginType>.Create<Plugin1>();
            var plugin1b = Plugin<IPluginType>.Create(new Plugin1());
            var plugin2 = Plugin<IPluginType>.Create<Plugin2>();
            var append = Plugin<IPluginType>.Create<Plugin3>();

            _plugins.Append(plugin1a).Append(plugin1b).Append(plugin2);

            _plugins.AppendAfterOrAppend<Plugin1>(append, @default)
                .ShouldOnlyContain(plugin1a, plugin1b, append, plugin2);

            _plugins.IsDefault(append).ShouldEqual(@default);
        }

        [Test]
        public void Should_append_plugin_after_last_of_type_when_it_exists_and_is_last()
        {
            var plugin1 = Plugin<IPluginType>.Create<Plugin1>();
            var append = Plugin<IPluginType>.Create<Plugin3>();

            _plugins.Append(plugin1);

            _plugins.AppendAfterOrAppend<Plugin2>(append)
                .ShouldOnlyContain(plugin1, append);
        }

        [Test]
        public void Should_append_plugin_after_last_of_type_when_it_exists_and_not_prepend(
            [Values(true, false)] bool @default)
        {
            var plugin1 = Plugin<IPluginType>.Create<Plugin1>();
            var plugin2 = Plugin<IPluginType>.Create<Plugin2>();
            var append = Plugin<IPluginType>.Create<Plugin3>();

            _plugins.Append(plugin1).Append(plugin2);

            _plugins.AppendAfterOrPrepend<Plugin1>(append, @default)
                .ShouldOnlyContain(plugin1, append, plugin2);

            _plugins.IsDefault(append).ShouldEqual(@default);
        }

        [Test]
        public void Should_append_plugin_when_it_cannot_be_appended_after_last_of_type(
            [Values(true, false)] bool @default)
        {
            var plugin1 = Plugin<IPluginType>.Create<Plugin1>();
            var plugin2 = Plugin<IPluginType>.Create<Plugin2>();
            var append = Plugin<IPluginType>.Create<Plugin3>();

            _plugins.Append(plugin1).Append(plugin2);

            _plugins.AppendAfterOrAppend<Plugin4>(append, @default)
                .ShouldOnlyContain(plugin1, plugin2, append);

            _plugins.IsDefault(append).ShouldEqual(@default);
        }

        [Test]
        public void Should_prepend_plugin_when_it_cannot_be_appended_after_last_of_type(
            [Values(true, false)] bool @default)
        {
            var plugin1 = Plugin<IPluginType>.Create<Plugin1>();
            var plugin2 = Plugin<IPluginType>.Create<Plugin2>();
            var append = Plugin<IPluginType>.Create<Plugin3>();

            _plugins.Append(plugin1).Append(plugin2);

            _plugins.AppendAfterOrPrepend<Plugin4>(append, @default)
                .ShouldOnlyContain(append, plugin1, plugin2);

            _plugins.IsDefault(append).ShouldEqual(@default);
        }

        [Test]
        public void Should_append_plugin_after_specified_plugin_when_it_exists_and_not_append(
            [Values(true, false)] bool @default)
        {
            var plugin1 = Plugin<IPluginType>.Create<Plugin1>();
            var plugin2 = Plugin<IPluginType>.Create<Plugin2>();
            var append = Plugin<IPluginType>.Create<Plugin3>();

            _plugins.Append(plugin1).Append(plugin2);

            _plugins.AppendAfterOrAppend(append, plugin1, @default)
                .ShouldOnlyContain(plugin1, append, plugin2);

            _plugins.IsDefault(append).ShouldEqual(@default);
        }

        [Test]
        public void Should_append_plugin_after_specified_plugin_when_it_exists_and_is_last()
        {
            var plugin1 = Plugin<IPluginType>.Create<Plugin1>();
            var append = Plugin<IPluginType>.Create<Plugin3>();

            _plugins.Append(plugin1);

            _plugins.AppendAfterOrAppend(append, plugin1)
                .ShouldOnlyContain(plugin1, append);
        }

        [Test]
        public void Should_append_plugin_after_specified_plugin_when_it_exists_and_not_prepend(
            [Values(true, false)] bool @default)
        {
            var plugin1 = Plugin<IPluginType>.Create<Plugin1>();
            var plugin2 = Plugin<IPluginType>.Create<Plugin2>();
            var append = Plugin<IPluginType>.Create<Plugin3>();

            _plugins.Append(plugin1).Append(plugin2);

            _plugins.AppendAfterOrPrepend(append, plugin1, @default)
                .ShouldOnlyContain(plugin1, append, plugin2);

            _plugins.IsDefault(append).ShouldEqual(@default);
        }

        [Test]
        public void Should_append_plugin_when_it_cannot_be_appended_after_specified_plugin(
            [Values(true, false)] bool @default)
        {
            var plugin1 = Plugin<IPluginType>.Create<Plugin1>();
            var plugin2 = Plugin<IPluginType>.Create<Plugin2>();
            var append = Plugin<IPluginType>.Create<Plugin3>();
            var notFound = Plugin<IPluginType>.Create<Plugin3>();

            _plugins.Append(plugin1).Append(plugin2);

            _plugins.AppendAfterOrAppend(append, notFound, @default)
                .ShouldOnlyContain(plugin1, plugin2, append);

            _plugins.IsDefault(append).ShouldEqual(@default);
        }

        [Test]
        public void Should_prepend_plugin_when_it_cannot_be_appended_after_specified_plugin(
            [Values(true, false)] bool @default)
        {
            var plugin1 = Plugin<IPluginType>.Create<Plugin1>();
            var plugin2 = Plugin<IPluginType>.Create<Plugin2>();
            var append = Plugin<IPluginType>.Create<Plugin3>();
            var notFound = Plugin<IPluginType>.Create<Plugin3>();

            _plugins.Append(plugin1).Append(plugin2);

            _plugins.AppendAfterOrPrepend(append, notFound, @default)
                .ShouldOnlyContain(append, plugin1, plugin2);

            _plugins.IsDefault(append).ShouldEqual(@default);
        }

        [Test]
        public void Should_prepend_plugin(
            [Values(true, false)] bool @default)
        {
            var plugin1 = Plugin<IPluginType>.Create<Plugin1>();
            var pepend = Plugin<IPluginType>.Create<Plugin2>();

            _plugins.Append(plugin1);

            _plugins.Prepend(pepend, @default)
                .ShouldOnlyContain(pepend, plugin1);

            _plugins.IsDefault(pepend).ShouldEqual(@default);
        }

        [Test]
        public void Should_prepend_plugin_before_first_of_type_when_it_exists_and_not_append(
            [Values(true, false)] bool @default)
        {
            var plugin1a = Plugin<IPluginType>.Create<Plugin1>();
            var plugin1b = Plugin<IPluginType>.Create(new Plugin1());
            var plugin2 = Plugin<IPluginType>.Create<Plugin2>();
            var prepend = Plugin<IPluginType>.Create<Plugin3>();

            _plugins.Append(plugin1a).Append(plugin1b).Append(plugin2);

            _plugins.PrependBeforeOrAppend<Plugin1>(prepend, @default)
                .ShouldOnlyContain(prepend, plugin1a, plugin1b, plugin2);

            _plugins.IsDefault(prepend).ShouldEqual(@default);
        }

        [Test]
        public void Should_prepend_plugin_before_first_of_type_when_it_exists_and_not_prepend(
            [Values(true, false)] bool @default)
        {
            var plugin1 = Plugin<IPluginType>.Create<Plugin1>();
            var plugin2 = Plugin<IPluginType>.Create<Plugin2>();
            var prepend = Plugin<IPluginType>.Create<Plugin3>();

            _plugins.Append(plugin1).Append(plugin2);

            _plugins.PrependBeforeOrPrepend<Plugin1>(prepend, @default)
                .ShouldOnlyContain(prepend, plugin1, plugin2);

            _plugins.IsDefault(prepend).ShouldEqual(@default);
        }

        [Test]
        public void Should_append_plugin_when_it_cannot_be_prepended_before_first_of_type(
            [Values(true, false)] bool @default)
        {
            var plugin1 = Plugin<IPluginType>.Create<Plugin1>();
            var plugin2 = Plugin<IPluginType>.Create<Plugin2>();
            var prepend = Plugin<IPluginType>.Create<Plugin3>();

            _plugins.Append(plugin1).Append(plugin2);

            _plugins.PrependBeforeOrAppend<Plugin4>(prepend, @default)
                .ShouldOnlyContain(plugin1, plugin2, prepend);

            _plugins.IsDefault(prepend).ShouldEqual(@default);
        }

        [Test]
        public void Should_prepend_plugin_when_it_cannot_be_prepended_before_first_of_type(
            [Values(true, false)] bool @default)
        {
            var plugin1 = Plugin<IPluginType>.Create<Plugin1>();
            var plugin2 = Plugin<IPluginType>.Create<Plugin2>();
            var prepend = Plugin<IPluginType>.Create<Plugin3>();

            _plugins.Append(plugin1).Append(plugin2);

            _plugins.PrependBeforeOrPrepend<Plugin4>(prepend, @default)
                .ShouldOnlyContain(prepend, plugin1, plugin2);

            _plugins.IsDefault(prepend).ShouldEqual(@default);
        }

        [Test]
        public void Should_prepend_plugin_before_specified_plugin_when_it_exists_and_not_append(
            [Values(true, false)] bool @default)
        {
            var plugin1 = Plugin<IPluginType>.Create<Plugin1>();
            var plugin2 = Plugin<IPluginType>.Create<Plugin2>();
            var prepend = Plugin<IPluginType>.Create<Plugin3>();

            _plugins.Append(plugin1).Append(plugin2);

            _plugins.PrependBeforeOrAppend(prepend, plugin1, @default)
                .ShouldOnlyContain(prepend, plugin1, plugin2);

            _plugins.IsDefault(prepend).ShouldEqual(@default);
        }

        [Test]
        public void Should_prepend_plugin_before_specified_plugin_when_it_exists_and_not_prepend(
            [Values(true, false)] bool @default)
        {
            var plugin1 = Plugin<IPluginType>.Create<Plugin1>();
            var plugin2 = Plugin<IPluginType>.Create<Plugin2>();
            var prepend = Plugin<IPluginType>.Create<Plugin3>();

            _plugins.Append(plugin1).Append(plugin2);

            _plugins.PrependBeforeOrPrepend(prepend, plugin1, @default)
                .ShouldOnlyContain(prepend, plugin1, plugin2);

            _plugins.IsDefault(prepend).ShouldEqual(@default);
        }

        [Test]
        public void Should_append_plugin_when_it_cannot_be_prepended_before_specified_plugin(
            [Values(true, false)] bool @default)
        {
            var plugin1 = Plugin<IPluginType>.Create<Plugin1>();
            var plugin2 = Plugin<IPluginType>.Create<Plugin2>();
            var prepend = Plugin<IPluginType>.Create<Plugin3>();
            var notFound = Plugin<IPluginType>.Create<Plugin3>();

            _plugins.Append(plugin1).Append(plugin2);

            _plugins.PrependBeforeOrAppend(prepend, notFound, @default)
                .ShouldOnlyContain(plugin1, plugin2, prepend);

            _plugins.IsDefault(prepend).ShouldEqual(@default);
        }

        [Test]
        public void Should_prepend_plugin_when_it_cannot_be_prepended_before_specified_plugin(
            [Values(true, false)] bool @default)
        {
            var plugin1 = Plugin<IPluginType>.Create<Plugin1>();
            var plugin2 = Plugin<IPluginType>.Create<Plugin2>();
            var prepend = Plugin<IPluginType>.Create<Plugin3>();
            var notFound = Plugin<IPluginType>.Create<Plugin3>();

            _plugins.Append(plugin1).Append(plugin2);

            _plugins.PrependBeforeOrPrepend(prepend, notFound, @default)
                .ShouldOnlyContain(prepend, plugin1, plugin2);

            _plugins.IsDefault(prepend).ShouldEqual(@default);
        }

        [Test]
        public void Should_insert_plugin(
            [Values(true, false)] bool @default)
        {
            var plugin1 = Plugin<IPluginType>.Create<Plugin1>();
            var plugin2 = Plugin<IPluginType>.Create<Plugin2>();
            var plugin3 = Plugin<IPluginType>.Create<Plugin3>();

            _plugins.Append(plugin1).Append(plugin3);

            _plugins.Insert(plugin2, 1, @default);

            _plugins.ShouldOnlyContain(plugin1, plugin2, plugin3);

            _plugins.IsDefault(plugin2).ShouldEqual(@default);
        }

        [Test]
        public void Should_enumerate_plugins()
        {
            var plugin1 = Plugin<IPluginType>.Create<Plugin1>();
            var plugin2 = Plugin<IPluginType>.Create<Plugin2>();

            _plugins.Append(plugin1).Append(plugin2);

            _plugins.ShouldOnlyContain(plugin1, plugin2);
        }
    }
}
