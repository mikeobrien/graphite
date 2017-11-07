using System;
using System.Linq;
using Graphite.Binding;
using Graphite.Http;
using Graphite.Routing;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Binding
{
    [TestFixture]
    public class ParameterBinderTests
    {
        public class Model
        {
            public string Param1 { get; set; }
            [Name("fark")]
            public string Param2 { get; set; }
        }

        public class Handler
        {
            public void Params(Model model, string param1, [Name("fark")] int param2) { }
        }

        [Test]
        public void Should_bind_values_to_parameters()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, 0))
                .WithUrl("http://fark.com?param1=value1&fark=value2")
                .AddParameters("param1", "param2")
                .AddValueMapper1(x => MapResult.Success(x.Values.First()));

            var result = Bind(requestGraph);

            result.Status.ShouldEqual(BindingStatus.Success);

            requestGraph.ActionArguments.ShouldOnlyContain(null, "value1", "value2");

            requestGraph.ValueMapper1.AppliesToContext.Parameter.ShouldNotBeNull();
            requestGraph.ValueMapper1.AppliesToContext.Values.ShouldNotBeNull();

            requestGraph.ValueMapper1.MapContext.Parameter.ShouldNotBeNull();
            requestGraph.ValueMapper1.MapContext.Values.ShouldNotBeNull();
        }

        [Test]
        public void Should_bind_values_to_properties()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, 0))
                .WithUrl("http://fark.com?param1=value1&fark=value2")
                .AddModelParameters("model", "param1", "param2")
                .AddValueMapper1(x => MapResult.Success(x.Values.First()));

            var result = Bind(requestGraph);

            result.Status.ShouldEqual(BindingStatus.Success);

            var model = requestGraph.ActionArguments[0] as Model;
            model.ShouldNotBeNull();
            model.Param1.ShouldEqual("value1");
            model.Param2.ShouldEqual("value2");

            requestGraph.ActionArguments[1].ShouldBeNull();
            requestGraph.ActionArguments[2].ShouldBeNull();

            requestGraph.ValueMapper1.AppliesToContext.Parameter.ShouldNotBeNull();
            requestGraph.ValueMapper1.AppliesToContext.Values.ShouldNotBeNull();

            requestGraph.ValueMapper1.MapContext.Parameter.ShouldNotBeNull();
            requestGraph.ValueMapper1.MapContext.Values.ShouldNotBeNull();
        }

        [Test]
        public void Should_not_map_parameters_that_dont_match_any_action_parameters()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, 0))
                .WithUrl("http://fark.com?param1=value1&param3=value3")
                .AddParameters("param1", "param2")
                .AddValueMapper1(x => MapResult.Success(x.Values.First()));

            var result = Bind(requestGraph);

            result.Status.ShouldEqual(BindingStatus.Success);

            requestGraph.ActionArguments.ShouldOnlyContain(null, "value1", null);
        }

        [Test]
        public void Should_not_map_parameter_names()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, 0))
                .WithUrl("http://fark.com?param1=value1&param3=value3")
                .AddParameters("param1", "param2")
                .AddValueMapper1(x => MapResult.Success(x.Values.First()));

            var binder = requestGraph.GetParameterBinder<BindResult>();

            var result = Bind(requestGraph,
                x => x.Name.EndsWith("2") ? x.Name.Replace("2", "3") : x.Name);

            result.Status.ShouldEqual(BindingStatus.Success);

            requestGraph.ActionArguments.ShouldOnlyContain(null, "value1", "value3");
        }

        [Test]
        public void Should_not_map_parameters_that_arent_passed()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, 0))
                .WithUrl("http://fark.com?param1=value1")
                .AddParameters("param1", "param2")
                .AddValueMapper1(x => MapResult.Success(x.Values.First()));

            var result = Bind(requestGraph);

            result.Status.ShouldEqual(BindingStatus.Success);

            requestGraph.ActionArguments.ShouldOnlyContain(null, "value1", null);
        }

        [Test]
        public void Should_use_the_first_mapper_that_applies()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, 0))
                .WithUrl("http://fark.com?param1=value1&fark=value2")
                .AddParameters("param1", "param2")
                .AddValueMapper1(x => MapResult.Success(x.Values.First() + "mapper1"))
                .AddValueMapper2(x => MapResult.Success(x.Values.First() + "mapper2"));

            var result = Bind(requestGraph);

            result.Status.ShouldEqual(BindingStatus.Success);

            requestGraph.ActionArguments.ShouldOnlyContain(null, "value1mapper1", "value2mapper1");

            requestGraph.ValueMapper1.AppliesToCalled.ShouldBeTrue();
            requestGraph.ValueMapper1.MapCalled.ShouldBeTrue();

            requestGraph.ValueMapper2.AppliesToCalled.ShouldBeTrue();
            requestGraph.ValueMapper2.MapCalled.ShouldBeFalse();
        }

        [Test]
        public void Should_not_use_a_mapper_that_doesnt_apply_in_configuration()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, 0))
                .WithUrl("http://fark.com?param1=value1&fark=value2")
                .AddParameters("param1", "param2")
                .AddValueMapper1(x => MapResult.Success(x.Values.First() + "mapper1"), 
                    configAppliesTo: x => false)
                .AddValueMapper2(x => MapResult.Success(x.Values.First() + "mapper2"));

            var result = Bind(requestGraph);

            result.Status.ShouldEqual(BindingStatus.Success);

            requestGraph.ActionArguments.ShouldOnlyContain(null, "value1mapper2", "value2mapper2");

            requestGraph.ValueMapper1.AppliesToCalled.ShouldBeFalse();
            requestGraph.ValueMapper1.MapCalled.ShouldBeFalse();

            requestGraph.ValueMapper2.AppliesToCalled.ShouldBeTrue();
            requestGraph.ValueMapper2.MapCalled.ShouldBeTrue();
        }

        [Test]
        public void Should_not_use_a_mapper_that_doesnt_apply_at_runtime()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, 0))
                .WithUrl("http://fark.com?param1=value1&fark=value2")
                .AddParameters("param1", "param2")
                .AddValueMapper1(x => MapResult.Success(x.Values.First() + "mapper1"), instanceAppliesTo: x => false)
                .AddValueMapper2(x => MapResult.Success(x.Values.First() + "mapper2"));

            var result = Bind(requestGraph);

            result.Status.ShouldEqual(BindingStatus.Success);

            requestGraph.ActionArguments.ShouldOnlyContain(null, "value1mapper2", "value2mapper2");

            requestGraph.ValueMapper1.AppliesToCalled.ShouldBeTrue();
            requestGraph.ValueMapper1.MapCalled.ShouldBeFalse();

            requestGraph.ValueMapper2.AppliesToCalled.ShouldBeTrue();
            requestGraph.ValueMapper2.MapCalled.ShouldBeTrue();
        }

        [Test]
        public void Should_skip_mapping_if_no_mappers_apply()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, 0))
                .WithUrl("http://fark.com?param1=value1")
                .AddParameters("param1", "param2")
                .WithRequestParameter("model");

            var result = Bind(requestGraph);

            result.Status.ShouldEqual(BindingStatus.Success);

            requestGraph.ActionArguments.ShouldOnlyContain(null, null, null);
        }

        [Test]
        public void Should_throw_exception_if_configured_no_mappers_apply()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, 0))
                .WithUrl("http://fark.com?param1=value1")
                .AddParameters("param1", "param2")
                .WithRequestParameter("model");

            requestGraph.Configuration.FailIfNoMapperFound = true;

            var exception = Assert.Throws<MapperNotFoundException>(() => Bind(requestGraph));

            exception.Message.ShouldEqual("Unable to map 'value1' to type string for 'param1' " +
                "parameter on action Tests.Unit.Binding.ParameterBinderTests.Handler.Params.");
        }

        [Test]
        public void Should_return_failure_result_if_mapping_fails()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, 0))
                .WithUrl("http://fark.com?fark=fark")
                .AddParameters("param2")
                .WithRequestParameter("model")
                .WithContentType(MimeTypes.ApplicationFormUrlEncoded);
            requestGraph.AppendValueMapper(new SimpleTypeMapper(requestGraph.Configuration));

            var result = Bind(requestGraph);

            result.Status.ShouldEqual(BindingStatus.Failure);
            result.ErrorMessage.ShouldEqual("Parameter param2 value 'fark' is not formatted correctly. " +
                                            "Input string was not in a correct format.");
        }

        private BindResult Bind(RequestGraph requestGraph,
            Func<ActionParameter, string> mapName = null)
        {
            return requestGraph.GetParameterBinder<BindResult>()
                .Bind(requestGraph.GetQuerystringParameters(),
                    requestGraph.GetActionParameters(),
                    (p, v) => p.BindArgument(requestGraph.ActionArguments, v),
                    BindResult.Success, BindResult.Failure, mapName);
        }
    }
}
