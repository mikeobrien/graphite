using System.Linq;
using Graphite.Extensibility;
using NUnit.Framework;
using Should;

namespace Tests.Unit.Extensibility
{
    [TestFixture]
    public class ConditionalPluginsTests
    {
        public interface IPluginType { }
        public class Plugin1 : IPluginType { }
        public class Plugin1a : IPluginType { }
        public class Plugin1b : IPluginType { }
        public class Plugin2 : IPluginType { }
        public class Plugin3 : IPluginType { }
        public class Plugin4 : IPluginType { }

        public class Context
        {
            public int Value { get; set; }
        }

        [Test]
        public void Should_return_plugins_that_apply_to_context()
        {
            var plugins = new ConditionalPlugins<IPluginType, Context>(false)
                .Configure(x => x
                    .Append<Plugin1a>(c => c.Value == 1)
                    .Append<Plugin1b>(c => c.Value == 1)
                    .Append<Plugin2>(c => c.Value == 2)
                    .Append<Plugin3>());

            var apply = plugins.ThatApplyTo(new Context { Value = 1 }).ToList();

            apply.Count.ShouldEqual(3);
            apply[0].Type.ShouldEqual(typeof(Plugin1a));
            apply[1].Type.ShouldEqual(typeof(Plugin1b));
            apply[2].Type.ShouldEqual(typeof(Plugin3));

            apply = plugins.ThatApplyTo(new Context { Value = 2 }).ToList();

            apply.Count.ShouldEqual(2);
            apply[0].Type.ShouldEqual(typeof(Plugin2));
            apply[1].Type.ShouldEqual(typeof(Plugin3));
        }

        [Test]
        public void Should_return_plugin_instances_that_apply_to_context()
        {
            var plugin1a = new Plugin1a();
            var plugin1b = new Plugin1b();
            var plugin2 = new Plugin2();
            var plugin3 = new Plugin3();
            var plugin4 = new Plugin4();

            var instances = new IPluginType[] { plugin1a, plugin1b, plugin2, plugin3, plugin4 };

            var plugins = new ConditionalPlugins<IPluginType, Context>(false)
                .Configure(x => x
                    .Append<Plugin1a>(c => c.Value == 1)
                    .Append<Plugin1b>(c => c.Value == 1)
                    .Append<Plugin2>(c => c.Value == 2)
                    .Append<Plugin3>());

            var apply = plugins.ThatApplyTo(instances, new Context { Value = 1 }).ToList();

            apply.Count.ShouldEqual(3);
            apply[0].Instance.ShouldEqual(plugin1a);
            apply[1].Instance.ShouldEqual(plugin1b);
            apply[2].Instance.ShouldEqual(plugin3);

            apply = plugins.ThatApplyTo(instances, new Context { Value = 2 }).ToList();

            apply.Count.ShouldEqual(2);
            apply[0].Instance.ShouldEqual(plugin2);
            apply[1].Instance.ShouldEqual(plugin3);
        }
    }
}
