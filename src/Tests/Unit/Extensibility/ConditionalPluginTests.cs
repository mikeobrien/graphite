using Graphite.Extensibility;
using NUnit.Framework;
using Should;

namespace Tests.Unit.Extensibility
{
    [TestFixture]
    public class ConditionalPluginTests
    {
        public interface IPluginType { }
        public class Plugin : IPluginType { }
        public class Context { }

        [Test]
        public void Should_set_applies_to_on_type_pugin()
        {
            bool Predicate(Context x) => false;

            var plugin = ConditionalPlugin<IPluginType, Context>
                .Create<Plugin>(Predicate);

            plugin.AppliesTo.ShouldEqual(Predicate);
        }

        [Test]
        public void Should_set_applies_to_on_instance_pugin()
        {
            bool Predicate(Context x) => false;

            var plugin = ConditionalPlugin<IPluginType, Context>
                .Create(new Plugin(), Predicate);

            plugin.AppliesTo.ShouldEqual(Predicate);
        }

        [Test]
        public void Should_clone()
        {
            var plugin = ConditionalPlugin<IPluginType, Context>
                .Create<Plugin>(x => false, true);

            var clone = plugin.Clone();

            (clone == plugin).ShouldBeFalse();
            clone.ShouldEqual(plugin);

            clone.Singleton.ShouldEqual(plugin.Singleton);
            clone.HasInstance.ShouldEqual(plugin.HasInstance);
            clone.Type.ShouldEqual(plugin.Type);
            clone.Instance.ShouldEqual(plugin.Instance);
            clone.Dispose.ShouldEqual(plugin.Dispose);
            clone.AppliesTo.ShouldEqual(plugin.AppliesTo);
        }
    }
}
