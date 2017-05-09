using System.Linq;
using System.Threading.Tasks;
using Bender.Collections;
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
            public void MultiParams(string[] param1) { }
        }

        [Test]
        public void Should_only_apply_if_there_are_querystring_parmeters(
            [Values(true, false)] bool hasParameters)
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, null))
                    .WithUrl("http://fark.com?param1=value1&param2=value2")
                    .AddValueMapper1(x => x.Values);

            if (hasParameters)
            {
                requestGraph.AddQuerystringParameter("param1")
                    .AddQuerystringParameter("param2");
            }

            new QuerystringBinder(requestGraph.ValueMappers,
                requestGraph.Configuration).AppliesTo(requestGraph.GetRequestBinderContext())
                .ShouldEqual(hasParameters);
        }

        [Test]
        public async Task Should_bind_querystring_values()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, null))
                    .WithUrl("http://fark.com?param1=value1&param2=value2")
                    .AddQuerystringParameter("param1")
                    .AddQuerystringParameter("param2")
                    .AddValueMapper1(x => x.Values.First());

            var binder = new QuerystringBinder(requestGraph.ValueMappers, 
                requestGraph.Configuration);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain(null, "value1", "value2");

            requestGraph.ValueMapper1.AppliesToContext.RequestContext.ShouldNotBeNull();
            requestGraph.ValueMapper1.AppliesToContext.Parameter.ShouldNotBeNull();
            requestGraph.ValueMapper1.AppliesToContext.Values.ShouldNotBeNull();

            requestGraph.ValueMapper1.MapContext.RequestContext.ShouldNotBeNull();
            requestGraph.ValueMapper1.MapContext.Parameter.ShouldNotBeNull();
            requestGraph.ValueMapper1.MapContext.Values.ShouldNotBeNull();
        }

        [Test]
        public async Task Should_map_multiple_parameters_with_the_same_name_as_an_array()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.MultiParams(null))
                    .WithUrl("http://fark.com?param1=value1&param1=value2")
                    .AddQuerystringParameter("param1")
                    .AddValueMapper1(x => x.Values);

            var binder = new QuerystringBinder(requestGraph.ValueMappers,
                requestGraph.Configuration);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments[0].CastTo<string[]>()
                .ShouldOnlyContain("value1", "value2");
        }

        [Test]
        public async Task Should_map_parameters_that_were_not_passed_as_null()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, null))
                    .WithUrl("http://fark.com?param1=value1")
                    .AddQuerystringParameter("param1")
                    .AddValueMapper1(x => x.Values.First());

            var binder = new QuerystringBinder(requestGraph.ValueMappers,
                requestGraph.Configuration);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain(null, "value1", null);
        }

        [Test]
        public async Task Should_use_the_first_mapper_that_applies()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, null))
                    .WithUrl("http://fark.com?param1=value1&param2=value2")
                    .AddQuerystringParameter("param1")
                    .AddQuerystringParameter("param2")
                    .AddValueMapper1(x => x.Values.First() + "mapper1")
                    .AddValueMapper2(x => x.Values.First() + "mapper2");

            var binder = new QuerystringBinder(requestGraph.ValueMappers,
                requestGraph.Configuration);

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
                    .AddQuerystringParameter("param1")
                    .AddQuerystringParameter("param2")
                    .AddValueMapper1(x => x.Values.First() + "mapper1", configAppliesTo: x => false)
                    .AddValueMapper2(x => x.Values.First() + "mapper2");

            var binder = new QuerystringBinder(requestGraph.ValueMappers,
                requestGraph.Configuration);

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
                    .AddQuerystringParameter("param1")
                    .AddQuerystringParameter("param2")
                    .AddValueMapper1(x => x.Values.First() + "mapper1", instanceAppliesTo: x => false)
                    .AddValueMapper2(x => x.Values.First() + "mapper2");

            var binder = new QuerystringBinder(requestGraph.ValueMappers,
                requestGraph.Configuration);

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
                    .AddQuerystringParameter("param1")
                    .AddQuerystringParameter("param2");

            var binder = new QuerystringBinder(requestGraph.ValueMappers,
                requestGraph.Configuration);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain(null, null, null);
        }

        [Test]
        public async Task Should_map_null_if_there_are_no_parameters()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, null))
                    .AddQuerystringParameter("param1")
                    .AddQuerystringParameter("param2");

            var binder = new QuerystringBinder(requestGraph.ValueMappers,
                requestGraph.Configuration);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain(null, null, null);
        }
    }
}
