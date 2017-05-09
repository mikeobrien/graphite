using System.Linq;
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
            public string Param2 { get; set; }
        }

        public class Handler
        {
            public void Params(Model model, string param1, string param2) { }
        }

        [Test]
        public void Should_bind_values_to_parameters()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, null))
                .WithUrl("http://fark.com?param1=value1&param2=value2")
                .AddParameters("param1", "param2")
                .AddValueMapper1(x => x.Values.First());

            var binder = requestGraph.GetParameterBinder();

            binder.Bind(requestGraph.GetQuerystringParameters(), 
                requestGraph.ActionArguments, 
                requestGraph.GetActionParameters(),
                x => x.Name);

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
                .CreateFor<Handler>(h => h.Params(null, null, null))
                .WithUrl("http://fark.com?param1=value1&param2=value2")
                .AddModelParameters("model", "param1", "param2")
                .AddValueMapper1(x => x.Values.First());

            var binder = requestGraph.GetParameterBinder();

            binder.Bind(requestGraph.GetQuerystringParameters(),
                requestGraph.ActionArguments,
                requestGraph.GetActionParameters(),
                x => x.Name);

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
                .CreateFor<Handler>(h => h.Params(null, null, null))
                .WithUrl("http://fark.com?param1=value1&param3=value3")
                .AddParameters("param1", "param2")
                .AddValueMapper1(x => x.Values.First());

            var binder = requestGraph.GetParameterBinder();

            binder.Bind(requestGraph.GetQuerystringParameters(),
                requestGraph.ActionArguments,
                requestGraph.GetActionParameters(),
                x => x.Name);

            requestGraph.ActionArguments.ShouldOnlyContain(null, "value1", null);
        }

        [Test]
        public void Should_not_map_parameter_names()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, null))
                .WithUrl("http://fark.com?param1=value1&param3=value3")
                .AddParameters("param1", "param2")
                .AddValueMapper1(x => x.Values.First());

            var binder = requestGraph.GetParameterBinder();

            binder.Bind(requestGraph.GetQuerystringParameters(),
                requestGraph.ActionArguments,
                requestGraph.GetActionParameters(),
                x => x.Name.EndsWith("2") ? x.Name.Replace("2", "3") : x.Name);

            requestGraph.ActionArguments.ShouldOnlyContain(null, "value1", "value3");
        }

        [Test]
        public void Should_not_map_parameters_that_arent_passed()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, null))
                .WithUrl("http://fark.com?param1=value1")
                .AddParameters("param1", "param2")
                .AddValueMapper1(x => x.Values.First());

            var binder = requestGraph.GetParameterBinder();

            binder.Bind(requestGraph.GetQuerystringParameters(),
                requestGraph.ActionArguments,
                requestGraph.GetActionParameters(),
                x => x.Name);

            requestGraph.ActionArguments.ShouldOnlyContain(null, "value1", null);
        }

        [Test]
        public void Should_use_the_first_mapper_that_applies()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, null))
                .WithUrl("http://fark.com?param1=value1&param2=value2")
                .AddParameters("param1", "param2")
                .AddValueMapper1(x => x.Values.First() + "mapper1")
                .AddValueMapper2(x => x.Values.First() + "mapper2");

            var binder = requestGraph.GetParameterBinder();

            binder.Bind(requestGraph.GetQuerystringParameters(),
                requestGraph.ActionArguments,
                requestGraph.GetActionParameters(),
                x => x.Name);

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
                .CreateFor<Handler>(h => h.Params(null, null, null))
                .WithUrl("http://fark.com?param1=value1&param2=value2")
                .AddParameters("param1", "param2")
                .AddValueMapper1(x => x.Values.First() + "mapper1", configAppliesTo: x => false)
                .AddValueMapper2(x => x.Values.First() + "mapper2");

            var binder = requestGraph.GetParameterBinder();

            binder.Bind(requestGraph.GetQuerystringParameters(),
                requestGraph.ActionArguments,
                requestGraph.GetActionParameters(),
                x => x.Name);

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
                .CreateFor<Handler>(h => h.Params(null, null, null))
                .WithUrl("http://fark.com?param1=value1&param2=value2")
                .AddParameters("param1", "param2")
                .AddValueMapper1(x => x.Values.First() + "mapper1", instanceAppliesTo: x => false)
                .AddValueMapper2(x => x.Values.First() + "mapper2");

            var binder = requestGraph.GetParameterBinder();

            binder.Bind(requestGraph.GetQuerystringParameters(),
                requestGraph.ActionArguments,
                requestGraph.GetActionParameters(),
                x => x.Name);

            requestGraph.ActionArguments.ShouldOnlyContain(null, "value1mapper2", "value2mapper2");

            requestGraph.ValueMapper1.AppliesToCalled.ShouldBeTrue();
            requestGraph.ValueMapper1.MapCalled.ShouldBeFalse();

            requestGraph.ValueMapper2.AppliesToCalled.ShouldBeTrue();
            requestGraph.ValueMapper2.MapCalled.ShouldBeTrue();
        }

        [Test]
        public void Should_not_bind_if_no_mappers_apply()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, null))
                .WithUrl("http://fark.com?param1=value1&param2=value2")
                .AddParameters("param1", "param2");

            var binder = requestGraph.GetParameterBinder();

            binder.Bind(requestGraph.GetQuerystringParameters(),
                requestGraph.ActionArguments,
                requestGraph.GetActionParameters(),
                x => x.Name);

            requestGraph.ActionArguments.ShouldOnlyContain(null, null, null);
        }

        [Test]
        public void Should_map_null_if_there_are_no_parameters()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, null))
                .WithRequestData("")
                .AddParameters("param1", "param2");

            var binder = requestGraph.GetParameterBinder();

            binder.Bind(requestGraph.GetQuerystringParameters(),
                requestGraph.ActionArguments,
                requestGraph.GetActionParameters(),
                x => x.Name);

            requestGraph.ActionArguments.ShouldOnlyContain(null, null, null);
        }
    }
}
