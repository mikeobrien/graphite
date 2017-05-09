using System;
using System.Collections.Generic;
using System.Linq;
using Graphite.Extensibility;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Extensibility
{
    [TestFixture]
    public class PluginDefinitionTests
    {
        private PluginDefinitions<ISomePlugin, SomePluginContext> _definitions;

        [SetUp]
        public void Setup()
        {
            _definitions = new PluginDefinitions<ISomePlugin, SomePluginContext>(singleton: false);
        }

        [Test]
        public void Should_select_plugin_defintions_that_apply_in_order()
        {
            var instance3 = new SomeImplementation3();
            _definitions
                .Append<SomeImplementation1>(x => x.SomeValue < 6)
                .Append<SomeImplementation2>(x => x.SomeValue > 5)
                .Append(instance3);

            var definitions = _definitions.ThatApplyTo(new SomePluginContext { SomeValue = 5 }).ToList();

            definitions.Count.ShouldEqual(2);
            definitions[0].HasInstance.ShouldBeFalse();
            definitions[0].Type.ShouldEqual(typeof(SomeImplementation1));
            definitions[1].HasInstance.ShouldBeTrue();
            definitions[1].Instance.ShouldEqual(instance3);
        }

        [Test]
        public void Should_select_plugin_instances_that_apply_in_order()
        {
            var instance3 = new SomeImplementation3();
            _definitions.Append<SomeImplementation4>()
                        .Append<SomeImplementation1>(x => x.SomeValue < 6)
                        .Append<SomeImplementation2>(x => x.SomeValue > 5)
                        .Append(instance3);
            var context = new SomePluginContext { SomeValue = 5 };
            var definitions = _definitions.ThatApplyTo(new List<ISomePlugin>
            {
                new SomeImplementation1(),
                new SomeImplementation2(),
                new SomeImplementation3(),
                new SomeImplementation3(),
                new SomeImplementation4(x => x.SomeValue > 5),
                new SomeImplementation5(x => x.SomeValue < 6),
                null
            }, context, context).ToList();

            definitions.Count.ShouldEqual(4);
            definitions[0].GetType().ShouldEqual(typeof(SomeImplementation1));
            definitions[1].GetType().ShouldEqual(typeof(SomeImplementation3));
            definitions[2].GetType().ShouldEqual(typeof(SomeImplementation3));
            definitions[3].GetType().ShouldEqual(typeof(SomeImplementation5));
        }

        [Test]
        public void Should_get_order_from_instance()
        {
            var instance2 = new SomeImplementation2();
            _definitions.Append<SomeImplementation1>()
                        .Append(instance2);

            _definitions.Order(new SomeImplementation1()).ShouldEqual(0);
            _definitions.Order(new SomeImplementation2()).ShouldEqual(1);
        }

        [Test]
        public void Should_get_order_from_generic_type_param()
        {
            var instance2 = new SomeImplementation2();
            _definitions.Append<SomeImplementation1>()
                        .Append(instance2);

            _definitions.Order<SomeImplementation1>().ShouldEqual(0);
            _definitions.Order<SomeImplementation2>().ShouldEqual(1);
        }

        [Test]
        public void Should_get_order_from_type()
        {
            var instance2 = new SomeImplementation2();
            _definitions.Append<SomeImplementation1>()
                        .Append(instance2);

            _definitions.Order(typeof(SomeImplementation1)).ShouldEqual(0);
            _definitions.Order(typeof(SomeImplementation2)).ShouldEqual(1);
        }

        [Test]
        public void Should_get_order_from_definition()
        {
            var instance2 = new SomeImplementation2();
            _definitions.Append<SomeImplementation1>()
                        .Append(instance2);

            _definitions.Order(_definitions.Get<SomeImplementation1>()).ShouldEqual(0);
            _definitions.Order(_definitions.Get<SomeImplementation2>()).ShouldEqual(1);
        }

        [Test]
        public void Should_get_definition_from_generic_type_param()
        {
            var instance2 = new SomeImplementation2();
            _definitions.Append<SomeImplementation1>()
                        .Append(instance2);

            _definitions.Get<SomeImplementation1>().Type.ShouldEqual(typeof(SomeImplementation1));
            _definitions.Get<SomeImplementation2>().Type.ShouldEqual(typeof(SomeImplementation2));
        }

        [Test]
        public void Should_get_definition_from_type()
        {
            var instance2 = new SomeImplementation2();
            _definitions.Append<SomeImplementation1>()
                        .Append(instance2);

            _definitions.Get(typeof(SomeImplementation1)).Type.ShouldEqual(typeof(SomeImplementation1));
            _definitions.Get(typeof(SomeImplementation2)).Type.ShouldEqual(typeof(SomeImplementation2));
        }

        [Test]
        public void Should_get_definition_from_instance()
        {
            var instance2 = new SomeImplementation2();
            _definitions.Append<SomeImplementation1>()
                        .Append(instance2);

            _definitions.Get(new SomeImplementation2()).Type.ShouldEqual(typeof(SomeImplementation2));
        }

        [Test]
        public void Should_clear_definitions()
        {
            _definitions.Append<SomeImplementation1>()
                        .Append<SomeImplementation2>();

            _definitions.Clear();

            _definitions.ShouldBeEmpty();
        }

        [Test]
        public void Should_indicate_if_a_definition_exists_by_type()
        {
            _definitions.Append<SomeImplementation1>();
            _definitions.Append(new SomeImplementation2());

            _definitions.Exists(typeof(SomeImplementation1)).ShouldBeTrue();
            _definitions.Exists(typeof(SomeImplementation2)).ShouldBeTrue();
            _definitions.Exists(typeof(SomeImplementation3)).ShouldBeFalse();
        }

        [Test]
        public void Should_indicate_if_a_definition_exists_by_generic_type_param()
        {
            _definitions.Append<SomeImplementation1>();
            _definitions.Append(new SomeImplementation2());

            _definitions.Exists<SomeImplementation1>().ShouldBeTrue();
            _definitions.Exists<SomeImplementation2>().ShouldBeTrue();
            _definitions.Exists<SomeImplementation3>().ShouldBeFalse();
        }

        [Test]
        public void Should_remove_a_definition()
        {
            var instance2 = new SomeImplementation2();
            _definitions.Append<SomeImplementation1>()
                        .Append(instance2)
                        .Append<SomeImplementation3>();

            _definitions.Remove<SomeImplementation3>();

            _definitions.Count().ShouldEqual(2);
            _definitions.Get<SomeImplementation1>().ShouldNotBeNull();
            _definitions.Get<SomeImplementation2>().ShouldNotBeNull();
            _definitions.Get<SomeImplementation3>().ShouldBeNull();
        }

        [Test]
        public void Should_not_fail_to_remove_a_definition_that_doesnt_exist()
        {
            _definitions.Should().NotThrow(x => x.Remove<SomeImplementation1>());
        }

        [Test]
        public void Should_replace_a_definition()
        {
            _definitions = new PluginDefinitions<ISomePlugin, SomePluginContext>(singleton: true);
            Func<SomePluginContext, bool> predicate4 = x => true;
            Func<SomePluginContext, bool> predicate5 = x => true;
            var instance2 = new SomeImplementation2();
            var instance4 = new SomeImplementation4();

            _definitions.Append<SomeImplementation1>()
                        .Append(instance2)
                        .Append<SomeImplementation3>();

            _definitions.Replace<SomeImplementation1>().With(instance4, predicate4);
            _definitions.Replace<SomeImplementation2>().With<SomeImplementation5>(predicate5);

            var definitions = _definitions.ToList();

            _definitions.Count().ShouldEqual(3);
            definitions[0].HasInstance.ShouldBeTrue();
            definitions[0].Instance.ShouldEqual(instance4);
            definitions[0].Singleton.ShouldBeFalse();
            definitions[0].AppliesTo.ShouldEqual(predicate4);
            definitions[1].HasInstance.ShouldBeFalse();
            definitions[1].Type.ShouldEqual(typeof(SomeImplementation5));
            definitions[1].Singleton.ShouldBeTrue();
            definitions[1].AppliesTo.ShouldEqual(predicate5);
            definitions[2].HasInstance.ShouldBeFalse();
            definitions[2].Type.ShouldEqual(typeof(SomeImplementation3));
        }

        [Test]
        public void Should_append_a_definition_if_replacent_doesent_exist()
        {
            var instance6 = new SomeImplementation6();
            _definitions.Append<SomeImplementation1>()
                        .Append<SomeImplementation2>();

            _definitions.Replace<SomeImplementation3>().With<SomeImplementation4>()
                .Replace<SomeImplementation5>().With(instance6);

            var definitions = _definitions.ToList();

            _definitions.Count().ShouldEqual(4);
            definitions[0].HasInstance.ShouldBeFalse();
            definitions[0].Type.ShouldEqual(typeof(SomeImplementation1));
            definitions[1].HasInstance.ShouldBeFalse();
            definitions[1].Type.ShouldEqual(typeof(SomeImplementation2));
            definitions[2].HasInstance.ShouldBeFalse();
            definitions[2].Type.ShouldEqual(typeof(SomeImplementation4));
            definitions[3].HasInstance.ShouldBeTrue();
            definitions[3].Instance.ShouldEqual(instance6);
        }

        [Test]
        public void Should_append_a_definition()
        {
            _definitions = new PluginDefinitions<ISomePlugin, SomePluginContext>(singleton: true);
            Func<SomePluginContext, bool> predicate1 = x => true;
            Func<SomePluginContext, bool> predicate2 = x => true;
            var instance2 = new SomeImplementation2();
            _definitions.Append<SomeImplementation1>(predicate1)
                        .Append(instance2, predicate2);

            var definitions = _definitions.ToList();

            _definitions.Count().ShouldEqual(2);
            definitions[0].HasInstance.ShouldBeFalse();
            definitions[0].Type.ShouldEqual(typeof(SomeImplementation1));
            definitions[0].Singleton.ShouldBeTrue();
            definitions[0].AppliesTo.ShouldEqual(predicate1);
            definitions[1].HasInstance.ShouldBeTrue();
            definitions[1].Instance.ShouldEqual(instance2);
            definitions[1].Singleton.ShouldBeFalse();
            definitions[1].AppliesTo.ShouldEqual(predicate2);
        }

        [Test]
        public void Should_append_a_definition_and_remove_existing()
        {
            var instance2 = new SomeImplementation2();
            _definitions.Prepend<SomeImplementation1>()
                        .Prepend<SomeImplementation2>();

            _definitions.Append<SomeImplementation1>()
                        .Append(instance2);

            var definitions = _definitions.ToList();

            _definitions.Count().ShouldEqual(2);
            definitions[0].HasInstance.ShouldBeFalse();
            definitions[0].Type.ShouldEqual(typeof(SomeImplementation1));
            definitions[1].HasInstance.ShouldBeTrue();
            definitions[1].Instance.ShouldEqual(instance2);
        }

        [Test]
        public void Should_append_a_definition_after_another()
        {
            var instance1 = new SomeImplementation1();
            var instance4 = new SomeImplementation4();
            _definitions.Append(instance1)
                        .Append<SomeImplementation2>();

            _definitions.Append<SomeImplementation3>().After<SomeImplementation1>();
            _definitions.Append(instance4).After<SomeImplementation2>();

            var definitions = _definitions.ToList();

            _definitions.Count().ShouldEqual(4);
            definitions[0].HasInstance.ShouldBeTrue();
            definitions[0].Instance.ShouldEqual(instance1);
            definitions[1].HasInstance.ShouldBeFalse();
            definitions[1].Type.ShouldEqual(typeof(SomeImplementation3));
            definitions[2].HasInstance.ShouldBeFalse();
            definitions[2].Type.ShouldEqual(typeof(SomeImplementation2));
            definitions[3].HasInstance.ShouldBeTrue();
            definitions[3].Instance.ShouldEqual(instance4);
        }

        [Test]
        public void Should_append_after_a_definition_and_remove_existing()
        {
            var instance2 = new SomeImplementation2();
            var instance3 = new SomeImplementation3();
            _definitions.Append<SomeImplementation1>()
                        .Append(instance2)
                        .Append<SomeImplementation3>()
                        .Append<SomeImplementation4>();

            _definitions.Append(instance3).After<SomeImplementation1>();
            _definitions.Append<SomeImplementation4>().After<SomeImplementation2>();

            var definitions = _definitions.ToList();
            
            _definitions.Count().ShouldEqual(4);
            definitions[0].HasInstance.ShouldBeFalse();
            definitions[0].Type.ShouldEqual(typeof(SomeImplementation1));
            definitions[1].HasInstance.ShouldBeTrue();
            definitions[1].Instance.ShouldEqual(instance3);
            definitions[2].HasInstance.ShouldBeTrue();
            definitions[2].Instance.ShouldEqual(instance2);
            definitions[3].HasInstance.ShouldBeFalse();
            definitions[3].Type.ShouldEqual(typeof(SomeImplementation4));
        }

        [Test]
        public void Should_prepend_a_plugin()
        {
            _definitions = new PluginDefinitions<ISomePlugin, SomePluginContext>(singleton: true);
            Func<SomePluginContext, bool> predicate1 = x => true;
            Func<SomePluginContext, bool> predicate2 = x => true;
            var instance2 = new SomeImplementation2();
            _definitions.Prepend<SomeImplementation1>(predicate1);
            _definitions.Prepend(instance2, predicate2);

            var definitions = _definitions.ToList();

            _definitions.Count().ShouldEqual(2);
            definitions[0].HasInstance.ShouldBeTrue();
            definitions[0].Instance.ShouldEqual(instance2);
            definitions[0].Singleton.ShouldBeFalse();
            definitions[0].AppliesTo.ShouldEqual(predicate2);
            definitions[1].HasInstance.ShouldBeFalse();
            definitions[1].Type.ShouldEqual(typeof(SomeImplementation1));
            definitions[1].Singleton.ShouldBeTrue();
            definitions[1].AppliesTo.ShouldEqual(predicate1);
        }

        [Test]
        public void Should_prepend_a_definition_and_remove_existing()
        {
            var instance2 = new SomeImplementation2();
            var instance3 = new SomeImplementation3();
            _definitions.Append<SomeImplementation1>()
                        .Append(instance2)
                        .Append<SomeImplementation3>();

            _definitions.Prepend<SomeImplementation2>()
                .Prepend(instance3);

            var definitions = _definitions.ToList();

            _definitions.Count().ShouldEqual(3);
            definitions[0].HasInstance.ShouldBeTrue();
            definitions[0].Instance.ShouldEqual(instance3);
            definitions[1].HasInstance.ShouldBeFalse();
            definitions[1].Type.ShouldEqual(typeof(SomeImplementation2));
            definitions[2].HasInstance.ShouldBeFalse();
            definitions[2].Type.ShouldEqual(typeof(SomeImplementation1));
        }

        [Test]
        public void Should_prepend_a_plugin_before_another()
        {
            var instance2 = new SomeImplementation2();
            var instance3 = new SomeImplementation3();
            _definitions.Append<SomeImplementation1>()
                        .Append(instance2);

            _definitions.Prepend(instance3)
                .Before<SomeImplementation1>();
            _definitions.Prepend<SomeImplementation4>()
                .Before<SomeImplementation2>();

            var definitions = _definitions.ToList();

            _definitions.Count().ShouldEqual(4);
            definitions[0].HasInstance.ShouldBeTrue();
            definitions[0].Instance.ShouldEqual(instance3);
            definitions[1].HasInstance.ShouldBeFalse();
            definitions[1].Type.ShouldEqual(typeof(SomeImplementation1));
            definitions[2].HasInstance.ShouldBeFalse();
            definitions[2].Type.ShouldEqual(typeof(SomeImplementation4));
            definitions[3].HasInstance.ShouldBeTrue();
            definitions[3].Instance.ShouldEqual(instance2);
        }

        [Test]
        public void Should_prepend_a_plugin_before_another_and_remove_existing()
        {
            var instance2 = new SomeImplementation2();
            var instance3 = new SomeImplementation3();

            _definitions.Append<SomeImplementation1>()
                        .Append(instance2)
                        .Append<SomeImplementation3>()
                        .Append<SomeImplementation4>();

            _definitions.Prepend(instance3).Before<SomeImplementation1>();
            _definitions.Prepend<SomeImplementation4>().Before<SomeImplementation2>();

            var definitions = _definitions.ToList();

            _definitions.Count().ShouldEqual(4);
            definitions[0].HasInstance.ShouldBeTrue();
            definitions[0].Instance.ShouldEqual(instance3);
            definitions[1].HasInstance.ShouldBeFalse();
            definitions[1].Type.ShouldEqual(typeof(SomeImplementation1));
            definitions[2].HasInstance.ShouldBeFalse();
            definitions[2].Type.ShouldEqual(typeof(SomeImplementation4));
            definitions[3].HasInstance.ShouldBeTrue();
            definitions[3].Instance.ShouldEqual(instance2);
        }

        public interface ISomePlugin : IConditional<SomePluginContext> { }

        public class SomePluginContext
        {
            public int SomeValue { get; set; }
        }

        public class SomeImplementationBase : ISomePlugin
        {
            private readonly Func<SomePluginContext, bool> _appliesTo;

            public SomeImplementationBase(Func<SomePluginContext, bool> appliesTo)
            {
                _appliesTo = appliesTo;
            }

            public bool AppliesTo(SomePluginContext context)
            {
                return _appliesTo?.Invoke(context) ?? true;
            }
        }

        public class SomeImplementation1 : SomeImplementationBase
        {
            public SomeImplementation1(Func<SomePluginContext, bool> appliesTo = null) : base(appliesTo) { }
        }

        public class SomeImplementation2 : SomeImplementationBase
        {
            public SomeImplementation2(Func<SomePluginContext, bool> appliesTo = null) : base(appliesTo) { }
        }

        public class SomeImplementation3 : SomeImplementationBase
        {
            public SomeImplementation3(Func<SomePluginContext, bool> appliesTo = null) : base(appliesTo) { }
        }

        public class SomeImplementation4 : SomeImplementationBase
        {
            public SomeImplementation4(Func<SomePluginContext, bool> appliesTo = null) : base(appliesTo) { }
        }

        public class SomeImplementation5 : SomeImplementationBase
        {
            public SomeImplementation5(Func<SomePluginContext, bool> appliesTo = null) : base(appliesTo) { }
        }

        public class SomeImplementation6 : SomeImplementationBase
        {
            public SomeImplementation6(Func<SomePluginContext, bool> appliesTo = null) : base(appliesTo) { }
        }
    }
}
