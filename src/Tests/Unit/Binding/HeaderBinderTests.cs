using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Http;
using Graphite.Binding;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Binding
{
    [TestFixture]
    public class HeaderBinderTests
    {
        private const string ParameterName = "value";
        private const string HeaderValue = "fark";

        public class Handler
        {
            public void ByName(string value) { }
            public void ByConvention(string valueHeader) { }
            public void ByAttribute([FromHeaders] string value) { }
            public void ByAttributeName([FromHeaders("value")] string someValue) { }
            public void ByNameWithFromUriAttribute([FromUri] string value) { }
            public void ByNameWithFromBodyAttribute([FromBody] string value) { }
        }

        public static object[][] BindingCases = TestCaseSource
            .CreateWithExpression<Handler, string, BindingMode, string>(x => x

                .Add(h => h.ByName(null), ParameterName, BindingMode.Implicit, HeaderValue)
                .Add(h => h.ByName(null), ParameterName, BindingMode.Explicit, null)
                .Add(h => h.ByName(null), ParameterName, BindingMode.None, null)
                .Add(h => h.ByName(null), ParameterName, BindingMode.Convention, null)

                .Add(h => h.ByConvention(null), "valueHeader", BindingMode.Implicit, null)
                .Add(h => h.ByConvention(null), "valueHeader", BindingMode.Explicit, null)
                .Add(h => h.ByConvention(null), "valueHeader", BindingMode.None, null)
                .Add(h => h.ByConvention(null), "valueHeader", BindingMode.Convention, HeaderValue)

                .Add(h => h.ByAttribute(null), ParameterName, BindingMode.Implicit, HeaderValue)
                .Add(h => h.ByAttribute(null), ParameterName, BindingMode.Explicit, HeaderValue)
                .Add(h => h.ByAttribute(null), ParameterName, BindingMode.None, null)
                .Add(h => h.ByAttribute(null), ParameterName, BindingMode.Convention, null)

                .Add(h => h.ByAttributeName(null), "someValue", BindingMode.Implicit, HeaderValue)
                .Add(h => h.ByAttributeName(null), "someValue", BindingMode.Explicit, HeaderValue)
                .Add(h => h.ByAttributeName(null), "someValue", BindingMode.None, null)
                .Add(h => h.ByAttributeName(null), "someValue", BindingMode.Convention, null)

                .Add(h => h.ByNameWithFromUriAttribute(null), ParameterName, BindingMode.Implicit, null)
                .Add(h => h.ByNameWithFromUriAttribute(null), ParameterName, BindingMode.Explicit, null)
                .Add(h => h.ByNameWithFromUriAttribute(null), ParameterName, BindingMode.None, null)
                .Add(h => h.ByNameWithFromUriAttribute(null), ParameterName, BindingMode.Convention, null)

                .Add(h => h.ByNameWithFromBodyAttribute(null), ParameterName, BindingMode.Implicit, null)
                .Add(h => h.ByNameWithFromBodyAttribute(null), ParameterName, BindingMode.Explicit, null)
                .Add(h => h.ByNameWithFromBodyAttribute(null), ParameterName, BindingMode.None, null)
                .Add(h => h.ByNameWithFromBodyAttribute(null), ParameterName, BindingMode.Convention, null)
            );

        [TestCaseSource(nameof(BindingCases))]
        public async Task Should_bind_headers(Expression<Action<Handler>> action,
            string parameter, BindingMode bindingMode, string expectedValue)
        {
            var requestGraph = RequestGraph
                .CreateFor(action)
                .AddParameters(parameter)
                .AddHeader("value", HeaderValue)
                .AddValueMapper(new SimpleTypeMapper());

            requestGraph.Configuration.HeadersBindingMode = bindingMode;

            await CreateBinder(requestGraph)
                .Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain(expectedValue);
        }

        public class MappingHandler
        {
            public void Action(string param1, string param2, string param3) { }
        }

        [Test]
        public async Task Should_use_the_first_mapper_that_applies()
        {
            var requestGraph = RequestGraph
                .CreateFor<MappingHandler>(h => h.Action(null, null, null))
                .AddAllActionParameters()
                .AddHeader("param2", "value2")
                .AddHeader("param3", "value3")
                .Configure(x => x.BindHeaders())
                .AddValueMapper1(x => x.Values.First() + "mapper1")
                .AddValueMapper2(x => x.Values.First() + "mapper2");

            var binder = CreateBinder(requestGraph);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain(null, "value2mapper1", "value3mapper1");

            requestGraph.ValueMapper1.AppliesToCalled.ShouldBeTrue();
            requestGraph.ValueMapper1.MapCalled.ShouldBeTrue();

            requestGraph.ValueMapper2.AppliesToCalled.ShouldBeTrue();
            requestGraph.ValueMapper2.MapCalled.ShouldBeFalse();
        }

        [Test]
        public async Task Should_not_use_a_mapper_that_doesnt_apply_in_configuration()
        {
            var requestGraph = RequestGraph
                .CreateFor<MappingHandler>(h => h.Action(null, null, null))
                .AddAllActionParameters()
                .AddHeader("param2", "value2")
                .AddHeader("param3", "value3")
                .Configure(x => x.BindHeaders())
                .AddValueMapper1(x => x.Values.First() + "mapper1", configAppliesTo: x => false)
                .AddValueMapper2(x => x.Values.First() + "mapper2");

            var binder = CreateBinder(requestGraph);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain(null, "value2mapper2", "value3mapper2");

            requestGraph.ValueMapper1.AppliesToCalled.ShouldBeFalse();
            requestGraph.ValueMapper1.MapCalled.ShouldBeFalse();

            requestGraph.ValueMapper2.AppliesToCalled.ShouldBeTrue();
            requestGraph.ValueMapper2.MapCalled.ShouldBeTrue();
        }

        [Test]
        public async Task Should_not_use_a_mapper_that_doesnt_apply_at_runtime()
        {
            var requestGraph = RequestGraph
                .CreateFor<MappingHandler>(h => h.Action(null, null, null))
                .AddAllActionParameters()
                .AddHeader("param2", "value2")
                .AddHeader("param3", "value3")
                .Configure(x => x.BindHeaders())
                .AddValueMapper1(x => x.Values.First() + "mapper1", instanceAppliesTo: x => false)
                .AddValueMapper2(x => x.Values.First() + "mapper2");

            var binder = CreateBinder(requestGraph);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain(null, "value2mapper2", "value3mapper2");

            requestGraph.ValueMapper1.AppliesToCalled.ShouldBeTrue();
            requestGraph.ValueMapper1.MapCalled.ShouldBeFalse();

            requestGraph.ValueMapper2.AppliesToCalled.ShouldBeTrue();
            requestGraph.ValueMapper2.MapCalled.ShouldBeTrue();
        }

        [Test]
        public async Task Should_not_bind_if_no_mappers_apply()
        {
            var requestGraph = RequestGraph
                .CreateFor<MappingHandler>(h => h.Action(null, null, null))
                .AddAllActionParameters()
                .AddHeader("param2", "value2")
                .AddHeader("param3", "value3")
                .Configure(x => x.BindHeaders());

            var binder = CreateBinder(requestGraph);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain(null, null, null);
        }

        [Test]
        public async Task Should_map_null_if_there_are_no_parameters()
        {
            var requestGraph = RequestGraph
                .CreateFor<MappingHandler>(h => h.Action(null, null, null))
                .AddHeader("param2", "value2")
                .AddHeader("param3", "value3")
                .Configure(x => x.BindHeaders());

            var binder = CreateBinder(requestGraph);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain(null, null, null);
        }

        public HeaderBinder CreateBinder(RequestGraph requestGraph)
        {
            return new HeaderBinder(
                requestGraph.Configuration,
                requestGraph.GetRouteDescriptor(),
                requestGraph.GetParameterBinder(),
                requestGraph.GetHttpRequestMessage());
        }
    }
}

