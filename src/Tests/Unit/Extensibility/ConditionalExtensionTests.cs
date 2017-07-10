using System;
using System.Linq;
using Graphite.Extensibility;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Extensibility
{
    [TestFixture]
    public class ConditionalExtensionTests
    {
        public interface IConditionalPluginType : IConditional { }
        public interface IContextConditionalPluginType : IConditional<InstanceContext> { }

        public class ConditionalPlugin : ConditionalPluginBase { }
        public class ConditionalPlugin1 : ConditionalPluginBase { }
        public class ConditionalPlugin1a : ConditionalPluginBase { }
        public class ConditionalPlugin1b : ConditionalPluginBase { }
        public class ConditionalPlugin2 : ConditionalPluginBase { }
        public class ConditionalPlugin3 : ConditionalPluginBase { }
        public class ConditionalPlugin4 : ConditionalPluginBase { }

        public class ContextConditionalPlugin : ContextConditionalPluginBase { }
        public class ContextConditionalPlugin1 : ContextConditionalPluginBase { }
        public class ContextConditionalPlugin2 : ContextConditionalPluginBase { }
        public class ContextConditionalPlugin3 : ContextConditionalPluginBase { }
        public class ContextConditionalPlugin4 : ContextConditionalPluginBase { }

        public class PluginContext
        {
            public int Value { get; set; }
        }

        public class InstanceContext
        {
            public int Value { get; set; }
        }

        // Instance context only

        [Test]
        public void Should_return_instances_that_apply(
            [Values(true, false)] bool orDefault)
        {
            var plugin1 = new ConditionalPlugin1 { DoesApply = false };
            var plugin2 = new ConditionalPlugin2 { DoesApply = true };
            var plugin3 = new ConditionalPlugin3 { DoesApply = true };

            var instances = new IConditionalPluginType[] { plugin1, plugin3, plugin2 };

            var plugins = new Plugins<IConditionalPluginType>(false)
                .Configure(x => x
                    .Append<ConditionalPlugin1>()
                    .Append<ConditionalPlugin2>()
                    .Append<ConditionalPlugin3>());

            var apply = (orDefault
                ? plugins.ThatApplyOrDefault(instances)
                : plugins.ThatApply(instances)).ToList();

            apply.ShouldOnlyContain(plugin2, plugin3);
        }

        [Test]
        public void Should_return_default_instances_if_non_apply()
        {
            var plugin1 = new ConditionalPlugin1 { DoesApply = false };
            var plugin2 = new ConditionalPlugin2 { DoesApply = false };

            var instances = new IConditionalPluginType[] { plugin1, plugin2 };

            var plugins = new Plugins<IConditionalPluginType>(false)
                .Configure(x => x
                    .Append<ConditionalPlugin1>()
                    .Append<ConditionalPlugin2>(@default: true));

            var apply = plugins.ThatApplyOrDefault(instances).ToList();

            apply.ShouldOnlyContain(plugin2);
        }

        [Test]
        public void Should_return_instances_that_apply_to_instance_context(
            [Values(true, false)] bool orDefault)
        {
            var plugin1 = new ContextConditionalPlugin3 { DoesApplyTo = x => x.Value == 2 };
            var plugin2 = new ContextConditionalPlugin2 { DoesApplyTo = x => x.Value == 1 };
            var plugin3 = new ContextConditionalPlugin3 { DoesApplyTo = x => x.Value == 1 };

            var instances = new IContextConditionalPluginType[] { plugin1, plugin3, plugin2 };

            var plugins = new Plugins<IContextConditionalPluginType>(false)
                .Configure(x => x
                    .Append<ContextConditionalPlugin1>()
                    .Append<ContextConditionalPlugin2>()
                    .Append<ContextConditionalPlugin3>());

            var context = new InstanceContext { Value = 1 };
            var apply = (orDefault 
                ? plugins.ThatApplyToOrDefault(instances, context)
                : plugins.ThatApplyTo(instances, context)).ToList();

            apply.ShouldOnlyContain(plugin2, plugin3);
        }

        [Test]
        public void Should_return_default_instances_if_non_apply_to_instance_context()
        {
            var plugin1 = new ContextConditionalPlugin3 { DoesApplyTo = x => x.Value == 2 };
            var plugin2 = new ContextConditionalPlugin2 { DoesApplyTo = x => x.Value == 2 };

            var instances = new IContextConditionalPluginType[] { plugin1, plugin2 };

            var plugins = new Plugins<IContextConditionalPluginType>(false)
                .Configure(x => x
                    .Append<ContextConditionalPlugin1>()
                    .Append<ContextConditionalPlugin2>(@default: true));

            var context = new InstanceContext { Value = 1 };
            var apply = plugins.ThatApplyToOrDefault(instances, context).ToList();

            apply.ShouldOnlyContain(plugin2);
        }

        // Plugin context only

        [Test]
        public void Should_return_instances_that_apply_to_plugin_context()
        {
            var plugin1 = new ConditionalPlugin1 { DoesApply = false };
            var plugin2 = new ConditionalPlugin2 { DoesApply = true };
            var plugin3 = new ConditionalPlugin3 { DoesApply = true };

            var instances = new IConditionalPluginType[] { plugin1, plugin3, plugin2 };

            var plugins = new ConditionalPlugins<IConditionalPluginType, PluginContext>(false)
                .Configure(x => x
                    .Append<ConditionalPlugin1>()
                    .Append<ConditionalPlugin2>()
                    .Append<ConditionalPlugin3>(c => c.Value == 1));

            var apply = plugins.ThatApply(instances, new PluginContext { Value = 1 }).ToList();

            apply.ShouldOnlyContain(plugin2, plugin3);
        }

        [Test]
        public void Should_not_return_instances_by_plugin_context_that_are_not_registered_in_the_plugin_list(
            [Values(true, false)] bool isRegistered)
        {
            var plugin = new ConditionalPlugin { DoesApply = true };

            var instances = new IConditionalPluginType[] { plugin };

            var plugins = new ConditionalPlugins<IConditionalPluginType, PluginContext>(false);

            if (isRegistered)
                plugins.Configure(x => x
                    .Append<ConditionalPlugin>());

            var apply = plugins.ThatApply(instances, new PluginContext()).ToList();

            apply.Count.ShouldEqual(isRegistered ? 1 : 0);
            apply.Contains(plugin).ShouldEqual(isRegistered);
        }

        [Test]
        public void Should_not_return_plugins_that_dont_apply_to_plugin_context(
            [Values(true, false)] bool applies)
        {
            var plugin = new ConditionalPlugin { DoesApply = true };

            var instances = new IConditionalPluginType[] { plugin };

            var plugins = new ConditionalPlugins<IConditionalPluginType, PluginContext>(false)
                .Configure(x => x
                    .Append<ConditionalPlugin>(c => c.Value == 1));

            var apply = plugins.ThatApply(instances, new PluginContext
                { Value = applies ? 1 : 2 }).ToList();

            apply.Count.ShouldEqual(applies ? 1 : 0);
            apply.Contains(plugin).ShouldEqual(applies);
        }

        [Test]
        public void Should_not_return_instances_that_dont_apply_to_plugin_context(
            [Values(true, false)] bool applies)
        {
            var plugin = new ConditionalPlugin { DoesApply = applies };

            var instances = new IConditionalPluginType[] { plugin };

            var plugins = new ConditionalPlugins<IConditionalPluginType, PluginContext>(false)
                .Configure(x => x
                    .Append<ConditionalPlugin>());

            var apply = plugins.ThatApply(instances, new PluginContext()).ToList();

            apply.Count.ShouldEqual(applies ? 1 : 0);
            apply.Contains(plugin).ShouldEqual(applies);
        }

        [Test]
        public void Should_return_first_instance_that_applies_to_plugin_context()
        {
            var plugin1 = new ConditionalPlugin1 { DoesApply = false };
            var plugin2 = new ConditionalPlugin2 { DoesApply = true };
            var plugin3 = new ConditionalPlugin3 { DoesApply = true };

            var instances = new IConditionalPluginType[] { plugin1, plugin3, plugin2 };

            var plugins = new ConditionalPlugins<IConditionalPluginType, PluginContext>(false)
                .Configure(x => x
                    .Append<ConditionalPlugin1>(@default: true)
                    .Append<ConditionalPlugin2>(c => c.Value == 1)
                    .Append<ConditionalPlugin3>(c => c.Value == 1));

            var applies = plugins.FirstThatAppliesOrDefault(instances, new PluginContext { Value = 1 });

            applies.ShouldEqual(plugin2);
        }

        [Test]
        public void Should_return_null_if_no_instances_appy_to_plugin_context_and_no_default_is_specified()
        {
            var plugin = new ConditionalPlugin { DoesApply = false };

            var instances = new IConditionalPluginType[] { plugin };

            var plugins = new ConditionalPlugins<IConditionalPluginType, PluginContext>(false)
                .Configure(x => x
                    .Append<ConditionalPlugin>());

            var applies = plugins.FirstThatAppliesOrDefault(instances, new PluginContext { Value = 1 });

            applies.ShouldBeNull();
        }

        [Test]
        public void Should_return_last_default_if_no_instances_appy_to_plugin_context_and_a_default_is_specified()
        {
            var plugin1 = new ConditionalPlugin1 { DoesApply = false };
            var plugin2 = new ConditionalPlugin2 { DoesApply = false };
            var plugin3 = new ConditionalPlugin3 { DoesApply = false };

            var instances = new IConditionalPluginType[] { plugin1, plugin2, plugin3 };

            var plugins = new ConditionalPlugins<IConditionalPluginType, PluginContext>(false)
                .Configure(x => x
                    .Append<ConditionalPlugin1>()
                    .Append<ConditionalPlugin2>(@default: true)
                    .Append<ConditionalPlugin3>(@default: true));

            var applies = plugins.FirstThatAppliesOrDefault(instances, new PluginContext());

            applies.ShouldEqual(plugin3);
        }

        [Test]
        public void Should_return_instances_that_apply_to_plugin_context_when_they_exist_and_not_the_default()
        {
            var plugin1 = new ConditionalPlugin1 { DoesApply = false };
            var plugin2 = new ConditionalPlugin2 { DoesApply = true };
            var plugin3 = new ConditionalPlugin3 { DoesApply = true };

            var instances = new IConditionalPluginType[] { plugin1, plugin3, plugin2 };

            var plugins = new ConditionalPlugins<IConditionalPluginType, PluginContext>(false)
                .Configure(x => x
                    .Append<ConditionalPlugin1>(@default: true)
                    .Append<ConditionalPlugin2>(c => c.Value == 1)
                    .Append<ConditionalPlugin3>(c => c.Value == 1));

            var applies = plugins.ThatAppliesOrDefault(instances, new PluginContext { Value = 1 });

            applies.ShouldOnlyContain(plugin2, plugin3);
        }

        [Test]
        public void Should_return_empty_list_if_no_instances_appy_to_plugin_context_and_no_default_is_specified()
        {
            var plugin = new ConditionalPlugin { DoesApply = false };

            var instances = new IConditionalPluginType[] { plugin };

            var plugins = new ConditionalPlugins<IConditionalPluginType, PluginContext>(false)
                .Configure(x => x
                    .Append<ConditionalPlugin>());

            var applies = plugins.ThatAppliesOrDefault(instances, new PluginContext());

            applies.ShouldBeEmpty();
        }

        [Test]
        public void Should_return_list_only_containing_last_default_if_no_instances_appy_to_plugin_context_and_a_default_is_specified()
        {
            var plugin1 = new ConditionalPlugin1 { DoesApply = false };
            var plugin2 = new ConditionalPlugin2 { DoesApply = false };
            var plugin3 = new ConditionalPlugin3 { DoesApply = false };

            var instances = new IConditionalPluginType[] { plugin1, plugin2, plugin3 };

            var plugins = new ConditionalPlugins<IConditionalPluginType, PluginContext>(false)
                .Configure(x => x
                    .Append<ConditionalPlugin1>()
                    .Append<ConditionalPlugin2>(@default: true)
                    .Append<ConditionalPlugin3>(@default: true));

            var applies = plugins.ThatAppliesOrDefault(instances, new PluginContext());

            applies.ShouldOnlyContain(plugin3);
        }

        // Plugin and instance context

        [Test]
        public void Should_return_instances_that_apply_to_plugin_and_instance_context()
        {
            var plugin1 = new ContextConditionalPlugin1 { DoesApplyTo = c => c.Value == 1 };
            var plugin2 = new ContextConditionalPlugin2 { DoesApplyTo = c => c.Value == 2 };
            var plugin3 = new ContextConditionalPlugin3 { DoesApplyTo = c => c.Value == 2 };

            var instances = new IContextConditionalPluginType[] { plugin1, plugin3, plugin2 };

            var plugins = new ConditionalPlugins<IContextConditionalPluginType, PluginContext>(false)
                .Configure(x => x
                    .Append<ContextConditionalPlugin1>()
                    .Append<ContextConditionalPlugin2>()
                    .Append<ContextConditionalPlugin3>(c => c.Value == 1));

            var apply = plugins.ThatAppliesTo(instances, 
                new PluginContext { Value = 1 }, 
                new InstanceContext { Value = 2 }).ToList();

            apply.ShouldOnlyContain(plugin2, plugin3);
        }

        [Test]
        public void Should_not_return_instances_by_plugin_context_and_instance_that_are_not_registered_in_the_plugin_list(
            [Values(true, false)] bool isRegistered)
        {
            var plugin = new ContextConditionalPlugin { DoesApplyTo = c => true };

            var instances = new IContextConditionalPluginType[] { plugin };

            var plugins = new ConditionalPlugins<IContextConditionalPluginType, PluginContext>(false);

            if (isRegistered)
                plugins.Configure(x => x
                    .Append<ContextConditionalPlugin>());

            var apply = plugins.ThatAppliesTo(instances, new PluginContext(), new InstanceContext()).ToList();

            apply.Count.ShouldEqual(isRegistered ? 1 : 0);
            apply.Contains(plugin).ShouldEqual(isRegistered);
        }

        [Test]
        public void Should_not_return_plugins_that_dont_apply_to_plugin_and_instance_context(
            [Values(true, false)] bool applies)
        {
            var plugin = new ContextConditionalPlugin { DoesApplyTo = c => c.Value == 1 };

            var instances = new IContextConditionalPluginType[] { plugin };

            var plugins = new ConditionalPlugins<IContextConditionalPluginType, PluginContext>(false)
                .Configure(x => x
                    .Append<ContextConditionalPlugin>(c => c.Value == 1));

            var apply = plugins.ThatAppliesTo(instances, 
                new PluginContext { Value = applies ? 1 : 2 },
                new InstanceContext { Value = applies ? 1 : 2 }).ToList();

            apply.Count.ShouldEqual(applies ? 1 : 0);
            apply.Contains(plugin).ShouldEqual(applies);
        }

        [Test]
        public void Should_not_return_instances_that_dont_apply_to_plugin_and_instance_context(
            [Values(true, false)] bool applies)
        {
            var plugin = new ContextConditionalPlugin { DoesApplyTo = c => applies };

            var instances = new IContextConditionalPluginType[] { plugin };

            var plugins = new ConditionalPlugins<IContextConditionalPluginType, PluginContext>(false)
                .Configure(x => x
                    .Append<ContextConditionalPlugin>());

            var apply = plugins.ThatAppliesTo(instances, new PluginContext(), 
                new InstanceContext()).ToList();

            apply.Count.ShouldEqual(applies ? 1 : 0);
            apply.Contains(plugin).ShouldEqual(applies);
        }

        [Test]
        public void Should_return_first_instance_that_applies_to_plugin_and_instance_context()
        {
            var plugin1 = new ContextConditionalPlugin1 { DoesApplyTo = c => c.Value == 2 };
            var plugin2 = new ContextConditionalPlugin2 { DoesApplyTo = c => c.Value == 1 };
            var plugin3 = new ContextConditionalPlugin3 { DoesApplyTo = c => c.Value == 1 };

            var instances = new IContextConditionalPluginType[] { plugin1, plugin3, plugin2 };

            var plugins = new ConditionalPlugins<IContextConditionalPluginType, PluginContext>(false)
                .Configure(x => x
                    .Append<ContextConditionalPlugin1>(@default: true)
                    .Append<ContextConditionalPlugin2>(c => c.Value == 1)
                    .Append<ContextConditionalPlugin3>(c => c.Value == 1));

            var applies = plugins.FirstThatAppliesToOrDefault(instances, 
                new PluginContext { Value = 1 },
                new InstanceContext { Value = 1 });

            applies.ShouldEqual(plugin2);
        }

        [Test]
        public void Should_return_null_if_no_instances_appy_to_plugin_and_instance_context_and_no_default_is_specified()
        {
            var plugin = new ContextConditionalPlugin { DoesApplyTo = c => false };

            var instances = new IContextConditionalPluginType[] { plugin };

            var plugins = new ConditionalPlugins<IContextConditionalPluginType, PluginContext>(false)
                .Configure(x => x
                    .Append<ContextConditionalPlugin>());

            var applies = plugins.FirstThatAppliesToOrDefault(instances, 
                new PluginContext { Value = 1 },
                new InstanceContext { Value = 1 });

            applies.ShouldBeNull();
        }

        [Test]
        public void Should_return_last_default_if_no_instances_appy_to_plugin_and_instance_context_and_a_default_is_specified()
        {
            var plugin1 = new ContextConditionalPlugin1 { DoesApplyTo = c => false };
            var plugin2 = new ContextConditionalPlugin2 { DoesApplyTo = c => false };
            var plugin3 = new ContextConditionalPlugin3 { DoesApplyTo = c => false };

            var instances = new IContextConditionalPluginType[] { plugin1, plugin2, plugin3 };

            var plugins = new ConditionalPlugins<IContextConditionalPluginType, PluginContext>(false)
                .Configure(x => x
                    .Append<ContextConditionalPlugin1>()
                    .Append<ContextConditionalPlugin2>(@default: true)
                    .Append<ContextConditionalPlugin3>(@default: true));

            var applies = plugins.FirstThatAppliesToOrDefault(instances, 
                new PluginContext(), new InstanceContext());

            applies.ShouldEqual(plugin3);
        }

        [Test]
        public void Should_return_instances_that_apply_to_plugin_and_instance_context_when_they_exist_and_not_the_default()
        {
            var plugin1 = new ContextConditionalPlugin1 { DoesApplyTo = c => c.Value == 2 };
            var plugin2 = new ContextConditionalPlugin2 { DoesApplyTo = c => c.Value == 1};
            var plugin3 = new ContextConditionalPlugin3 { DoesApplyTo = c => c.Value == 1 };

            var instances = new IContextConditionalPluginType[] { plugin1, plugin3, plugin2 };

            var plugins = new ConditionalPlugins<IContextConditionalPluginType, PluginContext>(false)
                .Configure(x => x
                    .Append<ContextConditionalPlugin1>(@default: true)
                    .Append<ContextConditionalPlugin2>(c => c.Value == 1)
                    .Append<ContextConditionalPlugin3>(c => c.Value == 1));

            var applies = plugins.ThatAppliesToOrDefault(instances, 
                new PluginContext { Value = 1 },
                new InstanceContext { Value = 1 });

            applies.ShouldOnlyContain(plugin2, plugin3);
        }

        [Test]
        public void Should_return_empty_list_if_no_instances_appy_to_plugin_and_instance_context_and_no_default_is_specified()
        {
            var plugin = new ContextConditionalPlugin { DoesApplyTo = c => false };

            var instances = new IContextConditionalPluginType[] { plugin };

            var plugins = new ConditionalPlugins<IContextConditionalPluginType, PluginContext>(false)
                .Configure(x => x
                    .Append<ContextConditionalPlugin>());

            var applies = plugins.ThatAppliesToOrDefault(instances, 
                new PluginContext(), new InstanceContext());

            applies.ShouldBeEmpty();
        }

        [Test]
        public void Should_return_list_only_containing_last_default_if_no_instances_appy_to_plugin_and_instance_context_and_a_default_is_specified()
        {
            var plugin1 = new ContextConditionalPlugin1 { DoesApplyTo = c => false };
            var plugin2 = new ContextConditionalPlugin2 { DoesApplyTo = c => false };
            var plugin3 = new ContextConditionalPlugin3 { DoesApplyTo = c => false };

            var instances = new IContextConditionalPluginType[] { plugin1, plugin2, plugin3 };

            var plugins = new ConditionalPlugins<IContextConditionalPluginType, PluginContext>(false)
                .Configure(x => x
                    .Append<ContextConditionalPlugin1>()
                    .Append<ContextConditionalPlugin2>(@default: true)
                    .Append<ContextConditionalPlugin3>(@default: true));

            var applies = plugins.ThatAppliesToOrDefault(instances, 
                new PluginContext(), new InstanceContext());

            applies.ShouldOnlyContain(plugin3);
        }

        public abstract class ConditionalPluginBase :
            IConditionalPluginType
        {
            public bool DoesApply { get; set; }

            public bool Applies()
            {
                return DoesApply;
            }
        }

        public abstract class ContextConditionalPluginBase : IContextConditionalPluginType
        {
            public InstanceContext AppliesToContext { get; private set; }
            public Func<InstanceContext, bool> DoesApplyTo { get; set; }

            public bool AppliesTo(InstanceContext context)
            {
                AppliesToContext = context;
                return DoesApplyTo(context);
            }
        }
    }
}
