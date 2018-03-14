using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Graphite.Binding;
using Graphite.Extensions;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Binding
{
    [TestFixture]
    public class QuerystringBinderTests
    {
        public class Handler
        {
            public void Params(object request, string param1, string param2) { }
            public void ParamsWithNameOverride([Name("param1")] string param2) { }
            public void ParamsWithFromUriNameOverride([FromUri(Name = "param1")] string param2) { }
            public void MultiParams(string[] param1) { }
        }

        [Test]
        public void Should_only_apply_if_there_are_querystring_parmeters(
            [Values(true, false)] bool hasParameters)
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, null))
                    .WithUrl("http://fark.com?param1=value1&param2=value2")
                    .AddValueMapper1(x => MapResult.Success(x.Values));

            if (hasParameters)
            {
                requestGraph.AddParameters("param1", "param2");
            }

            CreateBinder(requestGraph)
                .AppliesTo(requestGraph.GetRequestBinderContext())
                .ShouldEqual(hasParameters);
        }

        [Test]
        public async Task Should_bind_querystring_values()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, null))
                    .WithUrl("http://fark.com?param1=value1&param2=value2")
                    .AddParameters("param1", "param2")
                    .AddValueMapper1(x => MapResult.Success(x.Values.First()));

            var binder = CreateBinder(requestGraph);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain(null, "value1", "value2");

            requestGraph.ValueMapper1.AppliesToContext.Parameter.ShouldNotBeNull();
            requestGraph.ValueMapper1.AppliesToContext.Values.ShouldNotBeNull();

            requestGraph.ValueMapper1.MapContext.Parameter.ShouldNotBeNull();
            requestGraph.ValueMapper1.MapContext.Values.ShouldNotBeNull();
        }

        [Test]
        public async Task Should_map_multiple_parameters_with_the_same_name_as_an_array()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.MultiParams(null))
                    .WithUrl("http://fark.com?param1=value1&param1=value2")
                    .AddParameters("param1")
                    .AddValueMapper1(x => MapResult.Success(x.Values));

            var binder = CreateBinder(requestGraph);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments[0].CastTo<object[]>()
                .ShouldOnlyContain("value1", "value2");
        }

        [Test]
        public async Task Should_not_map_parameters_that_dont_match_any_action_parameters()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, null))
                    .WithUrl("http://fark.com?param1=value1")
                    .AddParameters("param1")
                    .AddValueMapper1(x => MapResult.Success(x.Values.First()));

            var binder = CreateBinder(requestGraph);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain(null, "value1", null);
        }

        [Test]
        public async Task Should_use_the_first_mapper_that_applies()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, null))
                    .WithUrl("http://fark.com?param1=value1&param2=value2")
                    .AddParameters("param1", "param2")
                    .AddValueMapper1(x => MapResult.Success(x.Values.First() + "mapper1"))
                    .AddValueMapper2(x => MapResult.Success(x.Values.First() + "mapper2"));

            var binder = CreateBinder(requestGraph);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain(null, "value1mapper1", "value2mapper1");

            requestGraph.ValueMapper1.AppliesToCalled.ShouldBeTrue();
            requestGraph.ValueMapper1.MapCalled.ShouldBeTrue();

            requestGraph.ValueMapper2.AppliesToCalled.ShouldBeTrue();
            requestGraph.ValueMapper2.MapCalled.ShouldBeFalse();
        }

        [Test]
        public async Task Should_not_use_a_mapper_that_doesnt_apply_in_configuration()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, null))
                    .WithUrl("http://fark.com?param1=value1&param2=value2")
                    .AddParameters("param1", "param2")
                    .AddValueMapper1(x => MapResult.Success(x.Values.First() + "mapper1"), 
                        configAppliesTo: x => false)
                    .AddValueMapper2(x => MapResult.Success(x.Values.First() + "mapper2"));

            var binder = CreateBinder(requestGraph);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain(null, "value1mapper2", "value2mapper2");

            requestGraph.ValueMapper1.AppliesToCalled.ShouldBeFalse();
            requestGraph.ValueMapper1.MapCalled.ShouldBeFalse();

            requestGraph.ValueMapper2.AppliesToCalled.ShouldBeTrue();
            requestGraph.ValueMapper2.MapCalled.ShouldBeTrue();
        }

        [Test]
        public async Task Should_not_use_a_mapper_that_doesnt_apply_at_runtime()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, null))
                    .WithUrl("http://fark.com?param1=value1&param2=value2")
                    .AddParameters("param1", "param2")
                    .AddValueMapper1(x => MapResult.Success(x.Values.First() + "mapper1"), 
                        instanceAppliesTo: x => false)
                    .AddValueMapper2(x => MapResult.Success(x.Values.First() + "mapper2"));

            var binder = CreateBinder(requestGraph);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain(null, "value1mapper2", "value2mapper2");

            requestGraph.ValueMapper1.AppliesToCalled.ShouldBeTrue();
            requestGraph.ValueMapper1.MapCalled.ShouldBeFalse();

            requestGraph.ValueMapper2.AppliesToCalled.ShouldBeTrue();
            requestGraph.ValueMapper2.MapCalled.ShouldBeTrue();
        }

        [Test]
        public async Task Should_not_bind_if_no_mappers_apply()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, null))
                    .WithUrl("http://fark.com?param1=value1&param2=value2")
                    .AddParameters("param1", "param2");

            var binder = CreateBinder(requestGraph);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain(null, null, null);
        }

        [Test]
        public async Task Should_map_null_if_there_are_no_parameters()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, null))
                    .AddParameters("param1", "param2");

            var binder = CreateBinder(requestGraph);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain(null, null, null);
        }

        public QuerystringBinder CreateBinder(RequestGraph requestGraph)
        {
            return new QuerystringBinder(
                requestGraph.GetRouteDescriptor(),
                requestGraph.GetArgumentBinder(),
                requestGraph.GetQuerystringParameters());
        }
    }
}
