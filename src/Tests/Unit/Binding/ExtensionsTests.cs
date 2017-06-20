using System;
using System.Collections.Generic;
using System.Linq;
using Graphite;
using Graphite.Actions;
using Graphite.Binding;
using Graphite.Routing;
using NUnit.Framework;
using Should;
using Tests.Common;
using Tests.Common.Fakes;

namespace Tests.Unit.Binding
{
    [TestFixture]
    public class ExtensionsTests
    {
        [Test]
        public void Should_map_parameter_with_first_matching_mapper_applies_to()
        {
            var mappers = new List<IValueMapper>
            {
                new TestValueMapper { MapFunc = c => $"{c.Values.First()}1", AppliesToFunc = c => false },
                new TestValueMapper { MapFunc = c => $"{c.Values.First()}2" }
            };

            var result = mappers.Map(null, null, null, new object[] { "value" }, 
                new ConfigurationContext(new Configuration(), null));

            result.Mapped.ShouldBeTrue();
            result.Value.ShouldEqual("value2");
        }

        [Test]
        public void Should_map_parameter_with_first_matching_configuration_applies_to()
        {
            var mappers = new List<IValueMapper>
            {
                new TestValueMapper1 { MapFunc = c => $"{c.Values.First()}1" },
                new TestValueMapper2 { MapFunc = c => $"{c.Values.First()}2" }
            };
            var configuration = new Configuration();

            configuration.ValueMappers.Append<TestValueMapper1>(x => false);

            var result = mappers.Map(null, null, null, new object[] { "value" },
                new ConfigurationContext(configuration, null));

            result.Mapped.ShouldBeTrue();
            result.Value.ShouldEqual("value2");
        }

        [Test]
        public void Should_map_parameter_with_default_mapper_if_configured_and_no_mappers_apply()
        {
            var mappers = new List<IValueMapper>
            {
                new TestValueMapper1 { MapFunc = c => $"{c.Values.First()}1" },
                new TestValueMapper2 { MapFunc = c => $"{c.Values.First()}2" }
            };
            var configuration = new Configuration();

            configuration.ValueMappers.Append<TestValueMapper1>(x => false);
            configuration.ValueMappers.Append<TestValueMapper2>(x => false, true);

            var result = mappers.Map(null, null, null, new object[] { "value" },
                new ConfigurationContext(configuration, null));

            result.Mapped.ShouldBeTrue();
            result.Value.ShouldEqual("value2");
        }

        [Test]
        public void Should_not_map_if_mapper_not_found()
        {
            var mappers = new List<IValueMapper>
            {
                new TestValueMapper { MapFunc = c => $"{c.Values.First()}1", AppliesToFunc = c => false }
            };

            var result = mappers.Map(null, null, null, new object[] { "value" },
                new ConfigurationContext(new Configuration(), null));

            result.Mapped.ShouldBeFalse();
            result.Value.ShouldBeNull();
        }

        [Test]
        public void Should_map_property_with_first_matching_configuration_applies_to()
        {
            var mappers = new List<IValueMapper>
            {
                new TestValueMapper1 { MapFunc = c => $"{c.Values.First()}1" },
                new TestValueMapper2 { MapFunc = c => $"{c.Values.First()}2" }
            };
            var configuration = new Configuration();

            configuration.ValueMappers.Append<TestValueMapper1>(x => false);

            var result = mappers.Map(null, null, null, new object[] { "value" },
                new ConfigurationContext(configuration, null));

            result.Mapped.ShouldBeTrue();
            result.Value.ShouldEqual("value2");
        }

        public class ArgumentHandler
        {
            public class Model { public string Value { get; set; } }

            public void Action(Model model, string value) { }
        }

        [Test]
        public void Should_get_value_from_argument()
        {
            var parameter = ActionMethod.From<ArgumentHandler>(x => x
                    .Action(null, null)).MethodDescriptor.Parameters
                .First(x => x.Name == "value");
            var actionParameter = new ActionParameter(parameter);
            var arguments = new object[] { null, "fark" };

            actionParameter.GetArgument(arguments).ShouldEqual("fark");
        }

        [Test]
        public void Should_get_value_from_argument_property()
        {
            var parameter = ActionMethod.From<ArgumentHandler>(x => x
                    .Action(null, null)).MethodDescriptor.Parameters
                .First(x => x.Name == "model");
            var property = parameter.ParameterType.Properties.First();
            var actionParameter = new ActionParameter(parameter, property);
            var arguments = new object[] { new ArgumentHandler.Model { Value = "fark" }, null };

            actionParameter.GetArgument(arguments).ShouldEqual("fark");
        }

        [Test]
        public void Should_return_null_if_property_argument_is_null()
        {
            var parameter = ActionMethod.From<ArgumentHandler>(x => x
                    .Action(null, null)).MethodDescriptor.Parameters
                .First(x => x.Name == "value");
            var property = parameter.ParameterType.Properties.First();
            var actionParameter = new ActionParameter(parameter, property);
            var arguments = new object[2];

            actionParameter.GetArgument(arguments).ShouldBeNull();
        }

        [Test]
        public void Should_bind_parameter_to_arguments()
        {
            var parameter = ActionMethod.From<ArgumentHandler>(x => x
                    .Action(null, null)).MethodDescriptor.Parameters
                .First(x => x.Name == "value");
            var actionParameter = new ActionParameter(parameter);
            var arguments = new object[2];

            actionParameter.BindArgument(arguments, "fark");

            arguments.ShouldOnlyContain(null, "fark");
        }

        [Test]
        public void Should_bind_new_parameter_property_to_arguments()
        {
            var parameter = ActionMethod.From<ArgumentHandler>(x => x
                .Action(null, null)).MethodDescriptor.Parameters
                .First(x => x.Name == "model");
            var property = parameter.ParameterType.Properties.First();
            var actionParameter = new ActionParameter(parameter, property);
            var arguments = new object[2];

            actionParameter.BindArgument(arguments, "fark");

            var model = arguments[0] as ArgumentHandler.Model;
            model.ShouldNotBeNull();
            model.Value.ShouldEqual("fark");

            arguments[1].ShouldBeNull();
        }

        [Test]
        public void Should_bind_existing_parameter_property_to_arguments()
        {
            var parameter = ActionMethod.From<ArgumentHandler>(x => x
                    .Action(null, null)).MethodDescriptor.Parameters
                .First(x => x.Name == "model");
            var property = parameter.ParameterType.Properties.First();
            var actionParameter = new ActionParameter(parameter, property);
            var arguments = new object[2];
            var existingModel = new ArgumentHandler.Model { Value = "farker" };
            arguments[0] = existingModel;

            actionParameter.BindArgument(arguments, "fark");

            var model = arguments[0] as ArgumentHandler.Model;
            model.ShouldNotBeNull();
            model.ShouldEqual(existingModel);
            model.Value.ShouldEqual("fark");

            arguments[1].ShouldBeNull();
        }

        [Test]
        public void Should_fail_to_bind_property_to_arguments()
        {
            var property = ActionMethod.From<ArgumentHandler>(x => x
                    .Action(null, null)).MethodDescriptor.Parameters
                .First(x => x.Name == "model").ParameterType.Properties.First();
            var actionParameter = new ActionParameter(property);
            var arguments = new object[2];
            
            actionParameter.Should().Throw<InvalidOperationException>(
                x => x.BindArgument(arguments, "fark"));
        }

        public class PropertyBinderHandler
        {
            public class Model { public string Value { get; set; } }

            public void Action(Model value) { }
        }

        [Test]
        public void Should_bind_property()
        {
            var property = ActionMethod.From<PropertyBinderHandler>(x => 
                x.Action(null)).MethodDescriptor.Parameters.First()
                .ParameterType.Properties.First();
            var actionParameter = new ActionParameter(property);
            var model = new PropertyBinderHandler.Model();

            actionParameter.BindProperty(model, "fark");

            model.Value.ShouldEqual("fark");
        }

        [Test]
        public void Should_bind_parameter_property()
        {
            var parameter = ActionMethod.From<PropertyBinderHandler>(x =>
                x.Action(null)).MethodDescriptor.Parameters.First();
            var property = parameter.ParameterType.Properties.First();
            var actionParameter = new ActionParameter(parameter, property);
            var model = new PropertyBinderHandler.Model();

            actionParameter.BindProperty(model, "fark");

            model.Value.ShouldEqual("fark");
        }

        [Test]
        public void Should_fail_to_bind_parameter()
        {
            var parameter = ActionMethod.From<PropertyBinderHandler>(x =>
                    x.Action(null)).MethodDescriptor.Parameters.First();
            var actionParameter = new ActionParameter(parameter);
            var model = new PropertyBinderHandler.Model();

            actionParameter.Should().Throw<InvalidOperationException>(
                x => x.BindProperty(model, "fark"));
        }
    }
}
