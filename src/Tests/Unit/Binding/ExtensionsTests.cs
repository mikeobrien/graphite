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
        private Configuration _configuration;

        [SetUp]
        public void Setup()
        {
            _configuration = new Configuration();
        }

        [Test]
        public void Should_map_parameter_with_first_matching_mapper_applies_to()
        {
            var mappers = new List<IValueMapper>
            {
                new TestValueMapper1
                {
                    MapFunc = c => MapResult.Success($"{c.Values.First()}1"),
                    AppliesToFunc = c => false
                },
                new TestValueMapper2
                {
                    MapFunc = c => MapResult.Success($"{c.Values.First()}2")
                }
            };

            _configuration.ValueMappers.Configure(c => c
                .Append<TestValueMapper1>()
                .Append<TestValueMapper2>());

            var result = mappers.Map(null, null, null, new object[] { "value" }, _configuration, null);

            result.Status.ShouldEqual(MappingStatus.Success);
            result.Value.ShouldEqual("value2");
        }

        [Test]
        public void Should_map_parameter_with_first_matching_configuration_applies_to()
        {
            var mappers = new List<IValueMapper>
            {
                new TestValueMapper1
                {
                    MapFunc = c => MapResult.Success($"{c.Values.First()}1")
                },
                new TestValueMapper2
                {
                    MapFunc = c => MapResult.Success($"{c.Values.First()}2")
                }
            };

            _configuration.ValueMappers.Configure(c => c
                .Append<TestValueMapper1>(x => false)
                .Append<TestValueMapper2>(x => true));

            var result = mappers.Map(null, null, null, new object[] { "value" }, _configuration, null);

            result.Status.ShouldEqual(MappingStatus.Success);
            result.Value.ShouldEqual("value2");
        }

        [Test]
        public void Should_map_parameter_with_default_mapper_if_configured_and_no_mappers_apply()
        {
            var mappers = new List<IValueMapper>
            {
                new TestValueMapper1
                {
                    MapFunc = c => MapResult.Success($"{c.Values.First()}1")
                },
                new TestValueMapper2
                {
                    MapFunc = c => MapResult.Success($"{c.Values.First()}2")
                }
            };

            _configuration.ValueMappers.Configure(c => c
                .Append<TestValueMapper1>(x => false)
                .Append<TestValueMapper2>(x => false, true));

            var result = mappers.Map(null, null, null, new object[] { "value" }, _configuration, null);

            result.Status.ShouldEqual(MappingStatus.Success);
            result.Value.ShouldEqual("value2");
        }

        [Test]
        public void Should_not_map_if_mapper_not_found()
        {
            var mappers = new List<IValueMapper>
            {
                new TestValueMapper
                {
                    MapFunc = c => MapResult.Success($"{c.Values.First()}1"), AppliesToFunc = c => false
                }
            };

            var result = mappers.Map(null, null, null, new object[] { "value" }, _configuration, null);

            result.Status.ShouldEqual(MappingStatus.NoMapper);
            result.Value.ShouldBeNull();
        }

        [Test]
        public void Should_map_property_with_first_matching_configuration_applies_to()
        {
            var mappers = new List<IValueMapper>
            {
                new TestValueMapper1
                {
                    MapFunc = c => MapResult.Success($"{c.Values.First()}1")
                },
                new TestValueMapper2
                {
                    MapFunc = c => MapResult.Success($"{c.Values.First()}2")
                }
            };

            _configuration.ValueMappers.Configure(c => c
                .Append<TestValueMapper1>(x => false)
                .Append<TestValueMapper2>(x => true));

            var result = mappers.Map(null, null, null, new object[] { "value" }, _configuration, null);

            result.Status.ShouldEqual(MappingStatus.Success);
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
            var action = ActionMethod.From<ArgumentHandler>(x => x.Action(null, null));
            var parameter = action.MethodDescriptor.Parameters
                .First(x => x.Name == "value");
            var actionParameter = new ActionParameter(action, parameter);
            var arguments = new object[] { null, "fark" };

            actionParameter.GetArgument(arguments).ShouldEqual("fark");
        }

        [Test]
        public void Should_get_value_from_argument_property()
        {
            var action = ActionMethod.From<ArgumentHandler>(x => x.Action(null, null));
            var parameter = action.MethodDescriptor.Parameters
                .First(x => x.Name == "model");
            var property = parameter.ParameterType.Properties.First();
            var actionParameter = new ActionParameter(action, parameter, property);
            var arguments = new object[] { new ArgumentHandler.Model { Value = "fark" }, null };

            actionParameter.GetArgument(arguments).ShouldEqual("fark");
        }

        [Test]
        public void Should_return_null_if_property_argument_is_null()
        {
            var action = ActionMethod.From<ArgumentHandler>(x => x.Action(null, null));
            var parameter = action.MethodDescriptor.Parameters
                .First(x => x.Name == "value");
            var property = parameter.ParameterType.Properties.First();
            var actionParameter = new ActionParameter(action, parameter, property);
            var arguments = new object[2];

            actionParameter.GetArgument(arguments).ShouldBeNull();
        }

        [Test]
        public void Should_bind_parameter_to_arguments()
        {
            var action = ActionMethod.From<ArgumentHandler>(x => x.Action(null, null));
            var parameter = action.MethodDescriptor.Parameters
                .First(x => x.Name == "value");
            var actionParameter = new ActionParameter(action, parameter);
            var arguments = new object[2];

            actionParameter.BindArgument(arguments, "fark");

            arguments.ShouldOnlyContain(null, "fark");
        }

        [Test]
        public void Should_bind_new_parameter_property_to_arguments()
        {
            var action = ActionMethod.From<ArgumentHandler>(x => x.Action(null, null));
            var parameter = action.MethodDescriptor.Parameters
                .First(x => x.Name == "model");
            var property = parameter.ParameterType.Properties.First();
            var actionParameter = new ActionParameter(action, parameter, property);
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
            var action = ActionMethod.From<ArgumentHandler>(x => x.Action(null, null));
            var parameter = action.MethodDescriptor.Parameters
                .First(x => x.Name == "model");
            var property = parameter.ParameterType.Properties.First();
            var actionParameter = new ActionParameter(action, parameter, property);
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
            var action = ActionMethod.From<ArgumentHandler>(x => x.Action(null, null));
            var property = action.MethodDescriptor.Parameters
                .First(x => x.Name == "model").ParameterType.Properties.First();
            var actionParameter = new ActionParameter(action, property);
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
            var action = ActionMethod.From<PropertyBinderHandler>(x => x.Action(null));
            var property = action.MethodDescriptor.Parameters.First()
                .ParameterType.Properties.First();
            var actionParameter = new ActionParameter(action, property);
            var model = new PropertyBinderHandler.Model();

            actionParameter.BindProperty(model, "fark");

            model.Value.ShouldEqual("fark");
        }

        [Test]
        public void Should_bind_parameter_property()
        {
            var action = ActionMethod.From<PropertyBinderHandler>(x => x.Action(null));
            var parameter = action.MethodDescriptor.Parameters.First();
            var property = parameter.ParameterType.Properties.First();
            var actionParameter = new ActionParameter(action, parameter, property);
            var model = new PropertyBinderHandler.Model();

            actionParameter.BindProperty(model, "fark");

            model.Value.ShouldEqual("fark");
        }

        [Test]
        public void Should_fail_to_bind_parameter()
        {
            var action = ActionMethod.From<PropertyBinderHandler>(x => x.Action(null));
            var parameter = action.MethodDescriptor.Parameters.First();
            var actionParameter = new ActionParameter(action, parameter);
            var model = new PropertyBinderHandler.Model();

            actionParameter.Should().Throw<InvalidOperationException>(
                x => x.BindProperty(model, "fark"));
        }
    }
}
