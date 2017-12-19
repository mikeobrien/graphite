using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Graphite;
using Graphite.Binding;
using Graphite.Extensions;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Binding
{
    [TestFixture]
    public class UrlParameterBinderTests
    {
        public class Handler
        {
            public void Params(object request, string param1, string param2) { }
            public void WildcardParams(object request, string param1, params int[] param2) { }
            public void WildcardArray(object request, string param1, int[] param2) { }
            public void WildcardList(object request, string param1, List<int> param2) { }
            public void WildcardIList(object request, string param1, IList<int> param2) { }
            public void WildcardIEnumerable(object request, string param1, IEnumerable<int> param2) { }
            public void WildcardICollection(object request, string param1, ICollection<int> param2) { }
        }
        
        private Configuration _configuration;

        [SetUp]
        public void Setup()
        {
            _configuration = new Configuration();
        }

        [TestCase("{param1}/segment/{param2}", "http://fark.com/value1/segment/value2", "value2")]
        [TestCase("{param1}/segment/{*param2}", "http://fark.com/value1/segment/value2/value3", "value2/value3")]
        public async Task Should_bind_url_parameter_values(string urlTemplate, string url, string lastValue)
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, null))
                    .WithUrl(url)
                    .WithUrlTemplate(urlTemplate)
                    .AddValueMapper1(x => MapResult.Success(x.Values.First()));

            var binder = CreateBinder(requestGraph);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain(null, "value1", lastValue);
            
            requestGraph.ValueMapper1.AppliesToContext.Parameter.ShouldNotBeNull();
            requestGraph.ValueMapper1.AppliesToContext.Values.ShouldNotBeNull();
            
            requestGraph.ValueMapper1.MapContext.Parameter.ShouldNotBeNull();
            requestGraph.ValueMapper1.MapContext.Values.ShouldNotBeNull();
        }

        public static object[][] WildcardCases = TestCaseSource
            .CreateWithExpression<Handler>(x => x
                .Add(h => h.WildcardParams(null, null))
                .Add(h => h.WildcardArray(null, null, null))
                .Add(h => h.WildcardList(null, null, null))
                .Add(h => h.WildcardIList(null, null, null))
                .Add(h => h.WildcardIEnumerable(null, null, null))
                .Add(h => h.WildcardICollection(null, null, null)));

        [TestCaseSource(nameof(WildcardCases))]
        public async Task Should_bind_generic_list_castable_wildcard_parameters(
            Expression<Action<Handler>> action)
        {
            var requestGraph = RequestGraph
                .CreateFor(action)
                    .WithUrl("http://fark.com/value1/segment/1/2/3")
                    .WithUrlTemplate("{param1}/segment/{*param2}")
                    .AddValueMapper(new SimpleTypeMapper(new ParsedValueMapper()));

            var binder = CreateBinder(requestGraph);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            var arguments = requestGraph.ActionArguments;
            arguments[0].ShouldBeNull();
            arguments[1].ShouldEqual("value1");
            arguments[2].As<IEnumerable<int>>().ToArray().ShouldEqual(new [] { 1, 2, 3 });
        }

        [Test]
        public async Task Should_use_the_first_mapper_that_applies()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, null))
                    .WithUrl("http://fark.com/value1/segment/value2")
                    .WithUrlTemplate("{param1}/segment/{param2}")
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
                    .WithUrl("http://fark.com/value1/segment/value2")
                    .WithUrlTemplate("{param1}/segment/{param2}")
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
                    .WithUrl("http://fark.com/value1/segment/value2")
                    .WithUrlTemplate("{param1}/segment/{param2}")
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
                    .WithUrl("http://fark.com/value1/segment/value2")
                    .WithUrlTemplate("{param1}/segment/{param2}");

            var binder = CreateBinder(requestGraph);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain(null, null, null);
        }

        [Test]
        public async Task Should_map_null_if_there_are_no_parameters()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, null))
                    .AddUrlParameter("param1")
                    .AddUrlParameter("param2");

            var binder = CreateBinder(requestGraph);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain(null, null, null);
        }

        public UrlParameterBinder CreateBinder(RequestGraph requestGraph)
        {
            return new UrlParameterBinder(
                requestGraph.GetRouteDescriptor(),
                requestGraph.GetArgumentBinder(),
                requestGraph.GetUrlParameters());
        }
    }
}
