using System;
using System.Linq;
using Graphite.Extensibility;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Extensibility
{
    [TestFixture]
    public class ConditionalPluginsDslTests
    {
        public interface IPluginType { }
        public class Plugin1 : IPluginType { }
        public class Plugin2 : IPluginType { }
        public class Plugin3 : IPluginType { }
        public class Context { }

        private readonly Func<Context, bool> _predicate = x => true;
        private ConditionalPlugins<IPluginType, Context> _plugins;
        private ConditionalPluginsDsl<IPluginType, Context> _pluginsDsl;

        [SetUp]
        public void Setup()
        {
            _plugins = new ConditionalPlugins<IPluginType, Context>(true);
            _pluginsDsl = new ConditionalPluginsDsl<IPluginType, Context>(_plugins);
        }

        [Test]
        public void Should_clear_plugins()
        {
            _plugins.Append(ConditionalPlugin<IPluginType, Context>
                .Create<Plugin1>(x => false));

            _plugins.Count().ShouldEqual(1);

            _pluginsDsl.Clear();

            _plugins.ShouldBeEmpty();
        }

        [Test]
        public void Should_clear_plugins_except_specified()
        {
            _plugins.Append(ConditionalPlugin<IPluginType, Context>.Create<Plugin1>(x => false));
            _plugins.Append(ConditionalPlugin<IPluginType, Context>.Create(new Plugin1(), x => false));
            _plugins.Append(ConditionalPlugin<IPluginType, Context>.Create<Plugin2>(x => false));

            _plugins.Count().ShouldEqual(3);

            _pluginsDsl.ClearExcept<Plugin2>();

            _plugins.Count().ShouldEqual(1);
            _plugins.First().Type.ShouldEqual(typeof(Plugin2));
        }

        [Test]
        public void Should_removed_specified_type()
        {
            _plugins.Append(ConditionalPlugin<IPluginType, Context>.Create<Plugin1>(x => false));
            _plugins.Append(ConditionalPlugin<IPluginType, Context>.Create<Plugin2>(x => false));
            _plugins.Append(ConditionalPlugin<IPluginType, Context>.Create(new Plugin2(), x => false));

            _plugins.Count().ShouldEqual(3);

            _pluginsDsl.Remove<Plugin2>();

            _plugins.Count().ShouldEqual(1);
            _plugins.First().Type.ShouldEqual(typeof(Plugin1));
        }

        [Test]
        public void Should_replace_specified_type_with_type_plugin(
            [Values(true, false)] bool @default,
            [Values(true, false)] bool when)
        {
            _plugins.Append(ConditionalPlugin<IPluginType, Context>.Create<Plugin1>(x => false));
            _plugins.Append(ConditionalPlugin<IPluginType, Context>.Create<Plugin2>(x => false));
            _plugins.Append(ConditionalPlugin<IPluginType, Context>.Create(new Plugin2(), x => false));

            RunInScope(when, (dsl, p) => dsl.Replace<Plugin2>().With<Plugin3>(p, @default));

            _plugins.Count().ShouldEqual(2);
            _plugins.First().Type.ShouldEqual(typeof(Plugin1));

            var replacement = _plugins.Second();
            replacement.Type.ShouldEqual(typeof(Plugin3));
            replacement.Singleton.ShouldBeTrue();
            replacement.AppliesTo.ShouldEqual(_predicate);

            _plugins.IsDefault(ConditionalPlugin<IPluginType, Context>
                .Create<Plugin3>(x => false))
                .ShouldEqual(@default);
        }

        [Test]
        public void Should_replace_specified_type_with_instance_plugin(
            [Values(true, false)] bool @default,
            [Values(true, false)] bool dispose,
            [Values(true, false)] bool when)
        {
            var instance3 = new Plugin3();
            _plugins.Append(ConditionalPlugin<IPluginType, Context>.Create<Plugin1>(x => false));
            _plugins.Append(ConditionalPlugin<IPluginType, Context>.Create<Plugin2>(x => false));
            _plugins.Append(ConditionalPlugin<IPluginType, Context>.Create(new Plugin2(), x => false));
            
            RunInScope(when, (dsl, p) => dsl.Replace<Plugin2>().With(instance3, p, dispose, @default));

            _plugins.Count().ShouldEqual(2);
            _plugins.First().Type.ShouldEqual(typeof(Plugin1));

            var replacement = _plugins.Second();
            replacement.Type.ShouldEqual(typeof(Plugin3));
            replacement.Instance.ShouldEqual(instance3);
            replacement.Dispose.ShouldEqual(dispose);
            replacement.AppliesTo.ShouldEqual(_predicate);

            _plugins.IsDefault(ConditionalPlugin<IPluginType, Context>
                .Create(instance3, x => false))
                .ShouldEqual(@default);
        }

        [Test]
        public void Should_append_type_plugin(
            [Values(true, false)] bool @default,
            [Values(true, false)] bool when)
        {
            var instance1 = new Plugin1();

            _pluginsDsl.Append(instance1);

            RunInScope(when, (dsl, p) => dsl.Append<Plugin2>(p, @default));

            _plugins.Count().ShouldEqual(2);

            _plugins.First().Instance.ShouldEqual(instance1);

            var appended = _plugins.Second();
            appended.Type.ShouldEqual(typeof(Plugin2));
            appended.Singleton.ShouldBeTrue();
            appended.AppliesTo.ShouldEqual(_predicate);

            _plugins.IsDefault(ConditionalPlugin<IPluginType, Context>
                .Create<Plugin2>(x => false))
                .ShouldEqual(@default);
        }

        [Test]
        public void Should_append_instance_plugin(
            [Values(true, false)] bool @default,
            [Values(true, false)] bool dispose,
            [Values(true, false)] bool when)
        {
            var instance2 = new Plugin2();

            _pluginsDsl.Append<Plugin1>();
            
            RunInScope(when, (dsl, p) => dsl.Append(instance2, p, dispose, @default));

            _plugins.Count().ShouldEqual(2);

            _plugins.First().Type.ShouldEqual(typeof(Plugin1));

            var appended = _plugins.Second();
            appended.Type.ShouldEqual(typeof(Plugin2));
            appended.Instance.ShouldEqual(instance2);
            appended.Dispose.ShouldEqual(dispose);
            appended.AppliesTo.ShouldEqual(_predicate);

            _plugins.IsDefault(ConditionalPlugin<IPluginType, Context>
                .Create(instance2, x => false))
                .ShouldEqual(@default);
        }

        [Test]
        public void Should_append_type_plugin_after_another(
            [Values(true, false)] bool @default,
            [Values(true, false)] bool orAppend,
            [Values(true, false)] bool when)
        {
            var instance1 = new Plugin1();

            _pluginsDsl.Append(instance1);
            _pluginsDsl.Append<Plugin2>();

            RunInScope(when, (dsl, p) =>
            {
                var appendDsl = dsl.Append<Plugin3>(_predicate, @default);
                
                if (orAppend) appendDsl.AfterOrAppend<Plugin1>();
                else appendDsl.AfterOrPrepend<Plugin1>();
            });
            
            _plugins.Count().ShouldEqual(3);

            _plugins.First().Type.ShouldEqual(typeof(Plugin1));

            var appended = _plugins.Second();
            appended.Type.ShouldEqual(typeof(Plugin3));
            appended.Singleton.ShouldBeTrue();
            appended.AppliesTo.ShouldEqual(_predicate);

            _plugins.Third().Type.ShouldEqual(typeof(Plugin2));

            _plugins.IsDefault(ConditionalPlugin<IPluginType, Context>
                .Create<Plugin3>(x => false))
                .ShouldEqual(@default);
        }

        [Test]
        public void Should_append_instance_plugin_after_another(
            [Values(true, false)] bool @default,
            [Values(true, false)] bool dispose,
            [Values(true, false)] bool orAppend,
            [Values(true, false)] bool when)
        {
            var instance3 = new Plugin3();

            _pluginsDsl.Append<Plugin1>();
            _pluginsDsl.Append<Plugin2>();

            RunInScope(when, (dsl, p) =>
            {
                var appendDsl = dsl.Append(instance3, _predicate, dispose, @default);

                if (orAppend) appendDsl.AfterOrAppend<Plugin1>();
                else appendDsl.AfterOrPrepend<Plugin1>();
            });

            _plugins.Count().ShouldEqual(3);

            _plugins.First().Type.ShouldEqual(typeof(Plugin1));

            var appended = _plugins.Second();
            appended.Type.ShouldEqual(typeof(Plugin3));
            appended.Instance.ShouldEqual(instance3);
            appended.Dispose.ShouldEqual(dispose);
            appended.AppliesTo.ShouldEqual(_predicate);

            _plugins.Third().Type.ShouldEqual(typeof(Plugin2));

            _plugins.IsDefault(ConditionalPlugin<IPluginType, Context>
                .Create(instance3, x => false))
                .ShouldEqual(@default);
        }

        [Test]
        public void Should_append_type_plugin_if_it_cannot_append_after_another(
            [Values(true, false)] bool @default,
            [Values(true, false)] bool when)
        {
            var instance1 = new Plugin1();

            _pluginsDsl.Append(instance1);

            RunInScope(when, (dsl, p) => dsl
                .Append<Plugin2>(p, @default)
                .AfterOrAppend<Plugin3>());

            _plugins.Count().ShouldEqual(2);

            _plugins.First().Instance.ShouldEqual(instance1);

            var appended = _plugins.Second();
            appended.Type.ShouldEqual(typeof(Plugin2));
            appended.Singleton.ShouldBeTrue();
            appended.AppliesTo.ShouldEqual(_predicate);

            _plugins.IsDefault(ConditionalPlugin<IPluginType, Context>
                .Create<Plugin2>(x => false))
                .ShouldEqual(@default);
        }

        [Test]
        public void Should_append_instance_plugin_if_it_cannot_append_after_another(
            [Values(true, false)] bool @default,
            [Values(true, false)] bool dispose,
            [Values(true, false)] bool when)
        {
            var instance2 = new Plugin2();

            _pluginsDsl.Append<Plugin1>();

            RunInScope(when, (dsl, p) => dsl
                .Append(instance2, p, dispose, @default)
                .AfterOrAppend<Plugin3>());

            _plugins.Count().ShouldEqual(2);

            _plugins.First().Type.ShouldEqual(typeof(Plugin1));

            var appended = _plugins.Second();
            appended.Type.ShouldEqual(typeof(Plugin2));
            appended.Instance.ShouldEqual(instance2);
            appended.Dispose.ShouldEqual(dispose);
            appended.AppliesTo.ShouldEqual(_predicate);

            _plugins.IsDefault(ConditionalPlugin<IPluginType, Context>
                .Create(instance2, x => false))
                .ShouldEqual(@default);
        }

        [Test]
        public void Should_prepend_type_plugin_if_it_cannot_append_after_another(
            [Values(true, false)] bool @default,
            [Values(true, false)] bool when)
        {
            var instance1 = new Plugin1();

            _pluginsDsl.Append(instance1);

            RunInScope(when, (dsl, p) => dsl
                .Append<Plugin2>(p, @default)
                .AfterOrPrepend<Plugin3>());

            _plugins.Count().ShouldEqual(2);

            var prepended = _plugins.First();
            prepended.Type.ShouldEqual(typeof(Plugin2));
            prepended.Singleton.ShouldBeTrue();
            prepended.AppliesTo.ShouldEqual(_predicate);

            _plugins.Second().Instance.ShouldEqual(instance1);

            _plugins.IsDefault(ConditionalPlugin<IPluginType, Context>
                .Create<Plugin2>(x => false))
                .ShouldEqual(@default);
        }

        [Test]
        public void Should_prepend_instance_plugin_if_it_cannot_append_after_another(
            [Values(true, false)] bool @default,
            [Values(true, false)] bool dispose,
            [Values(true, false)] bool when)
        {
            var instance2 = new Plugin2();

            _pluginsDsl.Append<Plugin1>();
            
            RunInScope(when, (dsl, p) => dsl
                .Append(instance2, p, dispose, @default)
                .AfterOrPrepend<Plugin3>());

            _plugins.Count().ShouldEqual(2);

            var prepended = _plugins.First();
            prepended.Type.ShouldEqual(typeof(Plugin2));
            prepended.Instance.ShouldEqual(instance2);
            prepended.Dispose.ShouldEqual(dispose);
            prepended.AppliesTo.ShouldEqual(_predicate);

            _plugins.Second().Type.ShouldEqual(typeof(Plugin1));

            _plugins.IsDefault(ConditionalPlugin<IPluginType, Context>
                .Create(instance2, x => false))
                .ShouldEqual(@default);
        }

        [Test]
        public void Should_prepend_type_plugin(
            [Values(true, false)] bool @default,
            [Values(true, false)] bool when)
        {
            var instance1 = new Plugin1();

            _pluginsDsl.Append(instance1);

            RunInScope(when, (dsl, p) => dsl.Prepend<Plugin2>(p, @default));

            _plugins.Count().ShouldEqual(2);

            var prepended = _plugins.First();
            prepended.Type.ShouldEqual(typeof(Plugin2));
            prepended.Singleton.ShouldBeTrue();
            prepended.AppliesTo.ShouldEqual(_predicate);

            _plugins.Second().Instance.ShouldEqual(instance1);

            _plugins.IsDefault(ConditionalPlugin<IPluginType, Context>
                .Create<Plugin2>(x => false))
                .ShouldEqual(@default);
        }

        [Test]
        public void Should_prepend_instance_plugin(
            [Values(true, false)] bool @default,
            [Values(true, false)] bool dispose,
            [Values(true, false)] bool when)
        {
            var instance2 = new Plugin2();

            _pluginsDsl.Append<Plugin1>();

            RunInScope(when, (dsl, p) => dsl.Prepend(instance2, p, dispose, @default));

            _plugins.Count().ShouldEqual(2);

            var prepended = _plugins.First();
            prepended.Type.ShouldEqual(typeof(Plugin2));
            prepended.Instance.ShouldEqual(instance2);
            prepended.Dispose.ShouldEqual(dispose);
            prepended.AppliesTo.ShouldEqual(_predicate);

            _plugins.Second().Type.ShouldEqual(typeof(Plugin1));

            _plugins.IsDefault(ConditionalPlugin<IPluginType, Context>
                .Create(instance2, x => false))
                .ShouldEqual(@default);
        }

        [Test]
        public void Should_prepend_type_plugin_before_another(
            [Values(true, false)] bool @default,
            [Values(true, false)] bool orPrepend,
            [Values(true, false)] bool when)
        {
            var instance1 = new Plugin1();

            _pluginsDsl.Append(instance1);
            _pluginsDsl.Append<Plugin2>();

            RunInScope(when, (dsl, p) =>
            {
                var prependDsl = dsl.Prepend<Plugin3>(p, @default);

                if (orPrepend) prependDsl.BeforeOrPrepend<Plugin2>();
                else prependDsl.BeforeOrAppend<Plugin2>();
            });

            _plugins.Count().ShouldEqual(3);

            _plugins.First().Instance.ShouldEqual(instance1);

            var prepended = _plugins.Second();
            prepended.Type.ShouldEqual(typeof(Plugin3));
            prepended.Singleton.ShouldBeTrue();
            prepended.AppliesTo.ShouldEqual(_predicate);

            _plugins.Third().Type.ShouldEqual(typeof(Plugin2));

            _plugins.IsDefault(ConditionalPlugin<IPluginType, Context>
                .Create<Plugin3>(x => false))
                .ShouldEqual(@default);
        }

        [Test]
        public void Should_prepend_instance_plugin_before_another(
            [Values(true, false)] bool @default,
            [Values(true, false)] bool dispose,
            [Values(true, false)] bool orPrepend,
            [Values(true, false)] bool when)
        {
            var instance3 = new Plugin3();

            _pluginsDsl.Append<Plugin1>();
            _pluginsDsl.Append<Plugin2>();
            
            RunInScope(when, (dsl, p) =>
            {
                var prependDsl = dsl.Prepend(instance3, p, dispose, @default);

                if (orPrepend) prependDsl.BeforeOrPrepend<Plugin2>();
                else prependDsl.BeforeOrAppend<Plugin2>();
            });

            _plugins.Count().ShouldEqual(3);

            _plugins.First().Type.ShouldEqual(typeof(Plugin1));

            var appended = _plugins.Second();
            appended.Type.ShouldEqual(typeof(Plugin3));
            appended.Instance.ShouldEqual(instance3);
            appended.Dispose.ShouldEqual(dispose);
            appended.AppliesTo.ShouldEqual(_predicate);

            _plugins.Third().Type.ShouldEqual(typeof(Plugin2));

            _plugins.IsDefault(ConditionalPlugin<IPluginType, Context>
                .Create(instance3, x => false))
                .ShouldEqual(@default);
        }

        [Test]
        public void Should_append_type_plugin_if_it_cannot_prepend_before_another(
            [Values(true, false)] bool @default,
            [Values(true, false)] bool when)
        {
            var instance1 = new Plugin1();

            _pluginsDsl.Append(instance1);

            RunInScope(when, (dsl, p) => dsl
                .Prepend<Plugin2>(p, @default)
                .BeforeOrAppend<Plugin3>());

            _plugins.Count().ShouldEqual(2);

            _plugins.First().Instance.ShouldEqual(instance1);

            var appended = _plugins.Second();
            appended.Type.ShouldEqual(typeof(Plugin2));
            appended.Singleton.ShouldBeTrue();
            appended.AppliesTo.ShouldEqual(_predicate);

            _plugins.IsDefault(ConditionalPlugin<IPluginType, Context>
                .Create<Plugin2>(x => false))
                .ShouldEqual(@default);
        }

        [Test]
        public void Should_append_instance_plugin_if_it_cannot_prepend_before_another(
            [Values(true, false)] bool @default,
            [Values(true, false)] bool dispose,
            [Values(true, false)] bool when)
        {
            var instance2 = new Plugin2();

            _pluginsDsl.Append<Plugin1>();

            RunInScope(when, (dsl, p) => dsl
                .Prepend(instance2, p, dispose, @default)
                .BeforeOrAppend<Plugin3>());

            _plugins.Count().ShouldEqual(2);

            _plugins.First().Type.ShouldEqual(typeof(Plugin1));

            var appended = _plugins.Second();
            appended.Type.ShouldEqual(typeof(Plugin2));
            appended.Instance.ShouldEqual(instance2);
            appended.Dispose.ShouldEqual(dispose);
            appended.AppliesTo.ShouldEqual(_predicate);

            _plugins.IsDefault(ConditionalPlugin<IPluginType, Context>
                .Create(instance2, x => false))
                .ShouldEqual(@default);
        }

        [Test]
        public void Should_prepend_type_plugin_if_it_cannot_prepend_before_another(
            [Values(true, false)] bool @default,
            [Values(true, false)] bool when)
        {
            var instance1 = new Plugin1();

            _pluginsDsl.Append(instance1);

            RunInScope(when, (dsl, p) => dsl
                .Prepend<Plugin2>(p, @default)
                .BeforeOrPrepend<Plugin3>());

            _plugins.Count().ShouldEqual(2);

            var prepended = _plugins.First();
            prepended.Type.ShouldEqual(typeof(Plugin2));
            prepended.Singleton.ShouldBeTrue();
            prepended.AppliesTo.ShouldEqual(_predicate);

            _plugins.Second().Instance.ShouldEqual(instance1);

            _plugins.IsDefault(ConditionalPlugin<IPluginType, Context>
                .Create<Plugin2>(x => false))
                .ShouldEqual(@default);
        }

        [Test]
        public void Should_prepend_instance_plugin_if_it_cannot_prepend_before_another(
            [Values(true, false)] bool @default,
            [Values(true, false)] bool dispose,
            [Values(true, false)] bool when)
        {
            var instance2 = new Plugin2();

            _pluginsDsl.Append<Plugin1>();

            RunInScope(when, (dsl, p) => dsl
                .Prepend(instance2, p, dispose, @default)
                .BeforeOrPrepend<Plugin3>());

            _plugins.Count().ShouldEqual(2);

            var prepended = _plugins.First();
            prepended.Type.ShouldEqual(typeof(Plugin2));
            prepended.Instance.ShouldEqual(instance2);
            prepended.Dispose.ShouldEqual(dispose);
            prepended.AppliesTo.ShouldEqual(_predicate);

            _plugins.Second().Type.ShouldEqual(typeof(Plugin1));

            _plugins.IsDefault(ConditionalPlugin<IPluginType, Context>
                .Create(instance2, x => false))
                .ShouldEqual(@default);
        }

        private void RunInScope(bool when,
            Action<ConditionalPluginsDsl<IPluginType, Context>,
                Func<Context, bool>> action)
        {
            if (when) _pluginsDsl.When(_predicate, x => action(x, null));
            else action(_pluginsDsl, _predicate);
        }
    }
}
