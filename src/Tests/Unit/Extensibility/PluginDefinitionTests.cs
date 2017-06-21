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
        private PluginDefinitions<IPluginWithContext, SomePluginContext> _definitionsWithContext;
        private PluginDefinitions<IPluginWithoutContext, SomePluginContext> _definitionsWithoutContext;

        [SetUp]
        public void Setup()
        {
            _definitionsWithContext = new PluginDefinitions<IPluginWithContext, SomePluginContext>(singleton: false);
            _definitionsWithoutContext = new PluginDefinitions<IPluginWithoutContext, SomePluginContext>(singleton: false);
        }

        [Test]
        public void Should_select_plugin_defintions_that_apply_in_order()
        {
            var instance3 = new PluginWithContext3();
            _definitionsWithContext
                .Append<PluginWithContext1>(x => x.SomeValue < 6)
                .Append<PluginWithContext2>(x => x.SomeValue > 5)
                .Append(instance3);

            var definitions = _definitionsWithContext.ThatApplyTo(new SomePluginContext { SomeValue = 5 }).ToList();

            definitions.Count.ShouldEqual(2);
            definitions[0].HasInstance.ShouldBeFalse();
            definitions[0].Type.ShouldEqual(typeof(PluginWithContext1));
            definitions[1].HasInstance.ShouldBeTrue();
            definitions[1].Instance.ShouldEqual(instance3);
        }

        [Test]
        public void Should_select_plugin_instances_that_apply_in_order()
        {
            var instance3 = new PluginWithContext3();
            _definitionsWithContext.Append<PluginWithContext4>()
                        .Append<PluginWithContext1>(x => x.SomeValue < 6)
                        .Append<PluginWithContext2>(x => x.SomeValue > 5)
                        .Append(instance3);
            var context = new SomePluginContext { SomeValue = 5 };
            var definitions = _definitionsWithContext.ThatApplyTo(new List<IPluginWithContext>
            {
                new PluginWithContext1(),
                new PluginWithContext2(),
                new PluginWithContext3(),
                new PluginWithContext3(),
                new PluginWithContext4(x => x.SomeValue > 5),
                new PluginWithContext5(x => x.SomeValue < 6),
                null
            }, context, context).ToList();

            definitions.Count.ShouldEqual(4);
            definitions[0].GetType().ShouldEqual(typeof(PluginWithContext1));
            definitions[1].GetType().ShouldEqual(typeof(PluginWithContext3));
            definitions[2].GetType().ShouldEqual(typeof(PluginWithContext3));
            definitions[3].GetType().ShouldEqual(typeof(PluginWithContext5));
        }

        [Test]
        public void Should_select_plugin_instance_that_applies_to_plugin_context_in_order()
        {
            var instance3 = new PluginWithContext3();
            _definitionsWithContext
                .Append<PluginWithContext4>()
                .Append<PluginWithContext1>(x => x.SomeValue < 6)
                .Append<PluginWithContext2>(x => x.SomeValue > 5)
                .Append(instance3);
            var context = new SomePluginContext { SomeValue = 5 };
            var definition = _definitionsWithContext.FirstThatAppliesToOrDefault(new List<IPluginWithContext>
            {
                new PluginWithContext1(),
                new PluginWithContext2(),
                new PluginWithContext3(),
                new PluginWithContext3(),
                new PluginWithContext4(x => x.SomeValue > 5),
                new PluginWithContext5(x => x.SomeValue < 6),
                null
            }, context, context);

            definition.GetType().ShouldEqual(typeof(PluginWithContext1));
        }

        [Test]
        public void Should_return_null_instance_if_none_applies_to_plugin_context_and_no_default_set()
        {
            _definitionsWithContext
                .Append<PluginWithContext1>(x => x.SomeValue > 6)
                .Append<PluginWithContext2>(x => x.SomeValue > 5);
            var context = new SomePluginContext { SomeValue = 4 };
            var definition = _definitionsWithContext.FirstThatAppliesToOrDefault(new List<IPluginWithContext>
            {
                new PluginWithContext1(),
                new PluginWithContext2()
            }, context, context);

            definition.ShouldBeNull();
        }

        [Test]
        public void Should_return_default_instance_if_none_applies_to_plugin_context_and_default_is_set()
        {
            _definitionsWithContext
                .Append<PluginWithContext1>(x => x.SomeValue > 6)
                .Append<PluginWithContext2>(x => x.SomeValue > 5, true);
            var context = new SomePluginContext { SomeValue = 4 };
            var definition = _definitionsWithContext.FirstThatAppliesToOrDefault(new List<IPluginWithContext>
            {
                new PluginWithContext1(),
                new PluginWithContext2()
            }, context, context);

            definition.ShouldNotBeNull();
            definition.GetType().ShouldEqual(typeof(PluginWithContext2));
        }

        [Test]
        public void Should_select_plugin_instance_that_applies_in_order()
        {
            var instance3 = new PluginWithoutContext3();
            _definitionsWithoutContext
                .Append<PluginWithoutContext4>()
                .Append<PluginWithoutContext1>(x => x.SomeValue < 6)
                .Append<PluginWithoutContext2>(x => x.SomeValue > 5)
                .Append(instance3);
            var context = new SomePluginContext { SomeValue = 4 };
            var definition = _definitionsWithoutContext.FirstThatAppliesToOrDefault(new List<IPluginWithoutContext>
            {
                new PluginWithoutContext1(),
                new PluginWithoutContext2(),
                new PluginWithoutContext3(),
                new PluginWithoutContext3(),
                new PluginWithoutContext4(() => false),
                new PluginWithoutContext5(() => true),
                null
            }, context);

            definition.GetType().ShouldEqual(typeof(PluginWithoutContext1));
        }

        [Test]
        public void Should_return_null_instance_if_none_applies_and_no_default_set()
        {
            _definitionsWithoutContext
                .Append<PluginWithoutContext1>(x => false)
                .Append<PluginWithoutContext2>(x => false);
            var definition = _definitionsWithoutContext.FirstThatAppliesToOrDefault(new List<IPluginWithoutContext>
            {
                new PluginWithoutContext1(),
                new PluginWithoutContext2()
            }, new SomePluginContext());

            definition.ShouldBeNull();
        }

        [Test]
        public void Should_return_default_instance_if_none_applies_and_default_is_set()
        {
            _definitionsWithoutContext
                .Append<PluginWithoutContext1>(x => false)
                .Append<PluginWithoutContext2>(x => false, true);
            var definition = _definitionsWithoutContext.FirstThatAppliesToOrDefault(new List<IPluginWithoutContext>
            {
                new PluginWithoutContext1(),
                new PluginWithoutContext2()
            }, new SomePluginContext());

            definition.ShouldNotBeNull();
            definition.GetType().ShouldEqual(typeof(PluginWithoutContext2));
        }

        [Test]
        public void Should_get_order_from_instance()
        {
            var instance2 = new PluginWithContext2();
            _definitionsWithContext.Append<PluginWithContext1>()
                        .Append(instance2);

            _definitionsWithContext.Order(new PluginWithContext1()).ShouldEqual(0);
            _definitionsWithContext.Order(new PluginWithContext2()).ShouldEqual(1);
        }

        [Test]
        public void Should_get_order_from_generic_type_param()
        {
            var instance2 = new PluginWithContext2();
            _definitionsWithContext.Append<PluginWithContext1>()
                        .Append(instance2);

            _definitionsWithContext.Order<PluginWithContext1>().ShouldEqual(0);
            _definitionsWithContext.Order<PluginWithContext2>().ShouldEqual(1);
        }

        [Test]
        public void Should_get_order_from_type()
        {
            var instance2 = new PluginWithContext2();
            _definitionsWithContext.Append<PluginWithContext1>()
                        .Append(instance2);

            _definitionsWithContext.Order(typeof(PluginWithContext1)).ShouldEqual(0);
            _definitionsWithContext.Order(typeof(PluginWithContext2)).ShouldEqual(1);
        }

        [Test]
        public void Should_get_order_from_definition()
        {
            var instance2 = new PluginWithContext2();
            _definitionsWithContext.Append<PluginWithContext1>()
                        .Append(instance2);

            _definitionsWithContext.Order(_definitionsWithContext.Get<PluginWithContext1>()).ShouldEqual(0);
            _definitionsWithContext.Order(_definitionsWithContext.Get<PluginWithContext2>()).ShouldEqual(1);
        }

        [Test]
        public void Should_get_definition_from_generic_type_param()
        {
            var instance2 = new PluginWithContext2();
            _definitionsWithContext.Append<PluginWithContext1>()
                        .Append(instance2);

            _definitionsWithContext.Get<PluginWithContext1>().Type.ShouldEqual(typeof(PluginWithContext1));
            _definitionsWithContext.Get<PluginWithContext2>().Type.ShouldEqual(typeof(PluginWithContext2));
        }

        [Test]
        public void Should_get_definition_from_type()
        {
            var instance2 = new PluginWithContext2();
            _definitionsWithContext.Append<PluginWithContext1>()
                        .Append(instance2);

            _definitionsWithContext.Get(typeof(PluginWithContext1)).Type.ShouldEqual(typeof(PluginWithContext1));
            _definitionsWithContext.Get(typeof(PluginWithContext2)).Type.ShouldEqual(typeof(PluginWithContext2));
        }

        [Test]
        public void Should_get_definition_from_instance()
        {
            var instance2 = new PluginWithContext2();
            _definitionsWithContext.Append<PluginWithContext1>()
                        .Append(instance2);

            _definitionsWithContext.Get(new PluginWithContext2()).Type.ShouldEqual(typeof(PluginWithContext2));
        }

        [Test]
        public void Should_clear_definitions()
        {
            _definitionsWithContext.Append<PluginWithContext1>()
                        .Append<PluginWithContext2>();

            _definitionsWithContext.Clear();

            _definitionsWithContext.ShouldBeEmpty();
        }

        [Test]
        public void Should_indicate_if_a_definition_exists_by_type()
        {
            _definitionsWithContext.Append<PluginWithContext1>();
            _definitionsWithContext.Append(new PluginWithContext2());

            _definitionsWithContext.Exists(typeof(PluginWithContext1)).ShouldBeTrue();
            _definitionsWithContext.Exists(typeof(PluginWithContext2)).ShouldBeTrue();
            _definitionsWithContext.Exists(typeof(PluginWithContext3)).ShouldBeFalse();
        }

        [Test]
        public void Should_indicate_if_a_definition_exists_by_generic_type_param()
        {
            _definitionsWithContext.Append<PluginWithContext1>();
            _definitionsWithContext.Append(new PluginWithContext2());

            _definitionsWithContext.Exists<PluginWithContext1>().ShouldBeTrue();
            _definitionsWithContext.Exists<PluginWithContext2>().ShouldBeTrue();
            _definitionsWithContext.Exists<PluginWithContext3>().ShouldBeFalse();
        }

        [Test]
        public void Should_remove_a_definition()
        {
            var instance2 = new PluginWithContext2();
            _definitionsWithContext.Append<PluginWithContext1>()
                        .Append(instance2)
                        .Append<PluginWithContext3>();

            _definitionsWithContext.Remove<PluginWithContext3>();

            _definitionsWithContext.Count().ShouldEqual(2);
            _definitionsWithContext.Get<PluginWithContext1>().ShouldNotBeNull();
            _definitionsWithContext.Get<PluginWithContext2>().ShouldNotBeNull();
            _definitionsWithContext.Get<PluginWithContext3>().ShouldBeNull();
        }

        [Test]
        public void Should_not_fail_to_remove_a_definition_that_doesnt_exist()
        {
            _definitionsWithContext.Should().NotThrow(x => x.Remove<PluginWithContext1>());
        }

        [Test]
        public void Should_replace_a_definition()
        {
            _definitionsWithContext = new PluginDefinitions<IPluginWithContext, SomePluginContext>(singleton: true);
            Func<SomePluginContext, bool> predicate4 = x => true;
            Func<SomePluginContext, bool> predicate5 = x => true;
            var instance2 = new PluginWithContext2();
            var instance4 = new PluginWithContext4();

            _definitionsWithContext.Append<PluginWithContext1>()
                        .Append(instance2)
                        .Append<PluginWithContext3>();

            _definitionsWithContext.Replace<PluginWithContext1>().With(instance4, predicate4);
            _definitionsWithContext.Replace<PluginWithContext2>().With<PluginWithContext5>(predicate5);

            var definitions = _definitionsWithContext.ToList();

            _definitionsWithContext.Count().ShouldEqual(3);
            definitions[0].HasInstance.ShouldBeTrue();
            definitions[0].Instance.ShouldEqual(instance4);
            definitions[0].Singleton.ShouldBeFalse();
            definitions[0].AppliesTo.ShouldEqual(predicate4);
            definitions[1].HasInstance.ShouldBeFalse();
            definitions[1].Type.ShouldEqual(typeof(PluginWithContext5));
            definitions[1].Singleton.ShouldBeTrue();
            definitions[1].AppliesTo.ShouldEqual(predicate5);
            definitions[2].HasInstance.ShouldBeFalse();
            definitions[2].Type.ShouldEqual(typeof(PluginWithContext3));
        }

        [Test]
        public void Should_append_a_definition_if_replacent_doesent_exist()
        {
            var instance6 = new PluginWithContext6();
            _definitionsWithContext.Append<PluginWithContext1>()
                        .Append<PluginWithContext2>();

            _definitionsWithContext.Replace<PluginWithContext3>().With<PluginWithContext4>()
                .Replace<PluginWithContext5>().With(instance6);

            var definitions = _definitionsWithContext.ToList();

            _definitionsWithContext.Count().ShouldEqual(4);
            definitions[0].HasInstance.ShouldBeFalse();
            definitions[0].Type.ShouldEqual(typeof(PluginWithContext1));
            definitions[1].HasInstance.ShouldBeFalse();
            definitions[1].Type.ShouldEqual(typeof(PluginWithContext2));
            definitions[2].HasInstance.ShouldBeFalse();
            definitions[2].Type.ShouldEqual(typeof(PluginWithContext4));
            definitions[3].HasInstance.ShouldBeTrue();
            definitions[3].Instance.ShouldEqual(instance6);
        }

        [Test]
        public void Should_append_a_definition()
        {
            _definitionsWithContext = new PluginDefinitions<IPluginWithContext, SomePluginContext>(singleton: true);
            Func<SomePluginContext, bool> predicate1 = x => true;
            Func<SomePluginContext, bool> predicate2 = x => true;
            var instance2 = new PluginWithContext2();
            _definitionsWithContext.Append<PluginWithContext1>(predicate1)
                        .Append(instance2, predicate2);

            var definitions = _definitionsWithContext.ToList();

            _definitionsWithContext.Count().ShouldEqual(2);
            definitions[0].HasInstance.ShouldBeFalse();
            definitions[0].Type.ShouldEqual(typeof(PluginWithContext1));
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
            var instance2 = new PluginWithContext2();
            _definitionsWithContext.Prepend<PluginWithContext1>()
                        .Prepend<PluginWithContext2>();

            _definitionsWithContext.Append<PluginWithContext1>()
                        .Append(instance2);

            var definitions = _definitionsWithContext.ToList();

            _definitionsWithContext.Count().ShouldEqual(2);
            definitions[0].HasInstance.ShouldBeFalse();
            definitions[0].Type.ShouldEqual(typeof(PluginWithContext1));
            definitions[1].HasInstance.ShouldBeTrue();
            definitions[1].Instance.ShouldEqual(instance2);
        }

        [Test]
        public void Should_append_a_definition_after_another()
        {
            var instance1 = new PluginWithContext1();
            var instance4 = new PluginWithContext4();
            _definitionsWithContext.Append(instance1)
                        .Append<PluginWithContext2>();

            _definitionsWithContext.Append<PluginWithContext3>().After<PluginWithContext1>();
            _definitionsWithContext.Append(instance4).After<PluginWithContext2>();

            var definitions = _definitionsWithContext.ToList();

            _definitionsWithContext.Count().ShouldEqual(4);
            definitions[0].HasInstance.ShouldBeTrue();
            definitions[0].Instance.ShouldEqual(instance1);
            definitions[1].HasInstance.ShouldBeFalse();
            definitions[1].Type.ShouldEqual(typeof(PluginWithContext3));
            definitions[2].HasInstance.ShouldBeFalse();
            definitions[2].Type.ShouldEqual(typeof(PluginWithContext2));
            definitions[3].HasInstance.ShouldBeTrue();
            definitions[3].Instance.ShouldEqual(instance4);
        }

        [Test]
        public void Should_append_after_a_definition_and_remove_existing()
        {
            var instance2 = new PluginWithContext2();
            var instance3 = new PluginWithContext3();
            _definitionsWithContext.Append<PluginWithContext1>()
                        .Append(instance2)
                        .Append<PluginWithContext3>()
                        .Append<PluginWithContext4>();

            _definitionsWithContext.Append(instance3).After<PluginWithContext1>();
            _definitionsWithContext.Append<PluginWithContext4>().After<PluginWithContext2>();

            var definitions = _definitionsWithContext.ToList();
            
            _definitionsWithContext.Count().ShouldEqual(4);
            definitions[0].HasInstance.ShouldBeFalse();
            definitions[0].Type.ShouldEqual(typeof(PluginWithContext1));
            definitions[1].HasInstance.ShouldBeTrue();
            definitions[1].Instance.ShouldEqual(instance3);
            definitions[2].HasInstance.ShouldBeTrue();
            definitions[2].Instance.ShouldEqual(instance2);
            definitions[3].HasInstance.ShouldBeFalse();
            definitions[3].Type.ShouldEqual(typeof(PluginWithContext4));
        }

        [Test]
        public void Should_prepend_a_plugin()
        {
            _definitionsWithContext = new PluginDefinitions<IPluginWithContext, SomePluginContext>(singleton: true);
            Func<SomePluginContext, bool> predicate1 = x => true;
            Func<SomePluginContext, bool> predicate2 = x => true;
            var instance2 = new PluginWithContext2();
            _definitionsWithContext.Prepend<PluginWithContext1>(predicate1);
            _definitionsWithContext.Prepend(instance2, predicate2);

            var definitions = _definitionsWithContext.ToList();

            _definitionsWithContext.Count().ShouldEqual(2);
            definitions[0].HasInstance.ShouldBeTrue();
            definitions[0].Instance.ShouldEqual(instance2);
            definitions[0].Singleton.ShouldBeFalse();
            definitions[0].AppliesTo.ShouldEqual(predicate2);
            definitions[1].HasInstance.ShouldBeFalse();
            definitions[1].Type.ShouldEqual(typeof(PluginWithContext1));
            definitions[1].Singleton.ShouldBeTrue();
            definitions[1].AppliesTo.ShouldEqual(predicate1);
        }

        [Test]
        public void Should_prepend_a_definition_and_remove_existing()
        {
            var instance2 = new PluginWithContext2();
            var instance3 = new PluginWithContext3();
            _definitionsWithContext.Append<PluginWithContext1>()
                        .Append(instance2)
                        .Append<PluginWithContext3>();

            _definitionsWithContext.Prepend<PluginWithContext2>()
                .Prepend(instance3);

            var definitions = _definitionsWithContext.ToList();

            _definitionsWithContext.Count().ShouldEqual(3);
            definitions[0].HasInstance.ShouldBeTrue();
            definitions[0].Instance.ShouldEqual(instance3);
            definitions[1].HasInstance.ShouldBeFalse();
            definitions[1].Type.ShouldEqual(typeof(PluginWithContext2));
            definitions[2].HasInstance.ShouldBeFalse();
            definitions[2].Type.ShouldEqual(typeof(PluginWithContext1));
        }

        [Test]
        public void Should_prepend_a_plugin_before_another()
        {
            var instance2 = new PluginWithContext2();
            var instance3 = new PluginWithContext3();
            _definitionsWithContext.Append<PluginWithContext1>()
                        .Append(instance2);

            _definitionsWithContext.Prepend(instance3)
                .Before<PluginWithContext1>();
            _definitionsWithContext.Prepend<PluginWithContext4>()
                .Before<PluginWithContext2>();

            var definitions = _definitionsWithContext.ToList();

            _definitionsWithContext.Count().ShouldEqual(4);
            definitions[0].HasInstance.ShouldBeTrue();
            definitions[0].Instance.ShouldEqual(instance3);
            definitions[1].HasInstance.ShouldBeFalse();
            definitions[1].Type.ShouldEqual(typeof(PluginWithContext1));
            definitions[2].HasInstance.ShouldBeFalse();
            definitions[2].Type.ShouldEqual(typeof(PluginWithContext4));
            definitions[3].HasInstance.ShouldBeTrue();
            definitions[3].Instance.ShouldEqual(instance2);
        }

        [Test]
        public void Should_prepend_a_plugin_before_another_and_remove_existing()
        {
            var instance2 = new PluginWithContext2();
            var instance3 = new PluginWithContext3();

            _definitionsWithContext.Append<PluginWithContext1>()
                        .Append(instance2)
                        .Append<PluginWithContext3>()
                        .Append<PluginWithContext4>();

            _definitionsWithContext.Prepend(instance3).Before<PluginWithContext1>();
            _definitionsWithContext.Prepend<PluginWithContext4>().Before<PluginWithContext2>();

            var definitions = _definitionsWithContext.ToList();

            _definitionsWithContext.Count().ShouldEqual(4);
            definitions[0].HasInstance.ShouldBeTrue();
            definitions[0].Instance.ShouldEqual(instance3);
            definitions[1].HasInstance.ShouldBeFalse();
            definitions[1].Type.ShouldEqual(typeof(PluginWithContext1));
            definitions[2].HasInstance.ShouldBeFalse();
            definitions[2].Type.ShouldEqual(typeof(PluginWithContext4));
            definitions[3].HasInstance.ShouldBeTrue();
            definitions[3].Instance.ShouldEqual(instance2);
        }

        public interface IPluginWithContext : IConditional<SomePluginContext> { }

        public class SomePluginContext
        {
            public int SomeValue { get; set; }
        }

        public class PluginWithContextBase : IPluginWithContext
        {
            private readonly Func<SomePluginContext, bool> _appliesTo;

            public PluginWithContextBase(Func<SomePluginContext, bool> appliesTo)
            {
                _appliesTo = appliesTo;
            }

            public bool AppliesTo(SomePluginContext context)
            {
                return _appliesTo?.Invoke(context) ?? true;
            }
        }

        public class PluginWithContext1 : PluginWithContextBase
        {
            public PluginWithContext1(Func<SomePluginContext, bool> appliesTo = null) : base(appliesTo) { }
        }

        public class PluginWithContext2 : PluginWithContextBase
        {
            public PluginWithContext2(Func<SomePluginContext, bool> appliesTo = null) : base(appliesTo) { }
        }

        public class PluginWithContext3 : PluginWithContextBase
        {
            public PluginWithContext3(Func<SomePluginContext, bool> appliesTo = null) : base(appliesTo) { }
        }

        public class PluginWithContext4 : PluginWithContextBase
        {
            public PluginWithContext4(Func<SomePluginContext, bool> appliesTo = null) : base(appliesTo) { }
        }

        public class PluginWithContext5 : PluginWithContextBase
        {
            public PluginWithContext5(Func<SomePluginContext, bool> appliesTo = null) : base(appliesTo) { }
        }

        public class PluginWithContext6 : PluginWithContextBase
        {
            public PluginWithContext6(Func<SomePluginContext, bool> appliesTo = null) : base(appliesTo) { }
        }

        public interface IPluginWithoutContext : IConditional { }

        public class PluginWithoutContextBase : IPluginWithoutContext
        {
            private readonly Func<bool> _applies;

            public PluginWithoutContextBase(Func<bool> applies)
            {
                _applies = applies;
            }

            public bool Applies()
            {
                return _applies?.Invoke() ?? true;
            }
        }

        public class PluginWithoutContext1 : PluginWithoutContextBase
        {
            public PluginWithoutContext1(Func<bool> applies = null) : base(applies) { }
        }

        public class PluginWithoutContext2 : PluginWithoutContextBase
        {
            public PluginWithoutContext2(Func<bool> applies = null) : base(applies) { }
        }

        public class PluginWithoutContext3 : PluginWithoutContextBase
        {
            public PluginWithoutContext3(Func<bool> applies = null) : base(applies) { }
        }

        public class PluginWithoutContext4 : PluginWithoutContextBase
        {
            public PluginWithoutContext4(Func<bool> applies = null) : base(applies) { }
        }

        public class PluginWithoutContext5 : PluginWithoutContextBase
        {
            public PluginWithoutContext5(Func<bool> applies = null) : base(applies) { }
        }

        public class PluginWithoutContext6 : PluginWithoutContextBase
        {
            public PluginWithoutContext6(Func<bool> applies = null) : base(applies) { }
        }
    }
}
