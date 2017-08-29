using System.Linq;
using System.Threading.Tasks;
using Graphite.Binding;
using Graphite.Http;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Binding
{
    [TestFixture]
    public class JsonBinderTests
    {
        public class Handler
        {
            public void Params(object request, string param1, string param2) { }
            public void MultiParams(string[] param1) { }
        }

        [Test]
        public void Should_only_apply_if_there_are_querystring_parmeters_on_the_action(
            [Values(true, false)] bool hasParameters)
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, null))
                    .WithRequestData("{\"param1\": \"value1\",\"param2\":\"value2\"}")
                    .WithContentType(MimeTypes.ApplicationJson)
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
        public void Should_only_apply_if_there_is_not_a_request_defined_for_the_action(
            [Values(true, false)] bool hasRequest)
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, null))
                    .WithRequestData("{\"param1\": \"value1\",\"param2\":\"value2\"}")
                    .AddParameters("param1", "param2")
                    .WithContentType(MimeTypes.ApplicationJson)
                    .AddValueMapper1(x => MapResult.Success(x.Values));

            if (hasRequest)
            {
                requestGraph.WithRequestParameter("request");
            }

            CreateBinder(requestGraph)
                .AppliesTo(requestGraph.GetRequestBinderContext())
                .ShouldEqual(!hasRequest);
        }

        [Test]
        public void Should_only_apply_if_the_content_type_is_json(
            [Values(true, false)] bool isJson)
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, null))
                    .WithRequestData("{\"param1\": \"value1\",\"param2\":\"value2\"}")
                    .AddParameters("param1", "param2")
                    .AddValueMapper1(x => MapResult.Success(x.Values));

            if (isJson)
            {
                requestGraph.WithContentType(MimeTypes.ApplicationJson);
            }

            CreateBinder(requestGraph)
                .AppliesTo(requestGraph.GetRequestBinderContext())
                .ShouldEqual(isJson);
        }

        [Test]
        public async Task Should_bind_json_values()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, null))
                    .WithRequestData("{\"param1\": \"value1\",\"param2\":\"value2\"}")
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
        public async Task Should_not_map_parameters_that_dont_match_any_action_parameters()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, null))
                    .WithRequestData("{\"param1\": \"value1\",\"param2\":\"value2\"}")
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
                    .WithRequestData("{\"param1\": \"value1\",\"param2\":\"value2\"}")
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
                    .WithRequestData("{\"param1\": \"value1\",\"param2\":\"value2\"}")
                    .AddParameters("param1", "param2")
                    .AddValueMapper1(x => MapResult.Success(x.Values.First() + "mapper1"), configAppliesTo: x => false)
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
                    .WithRequestData("{\"param1\": \"value1\",\"param2\":\"value2\"}")
                    .AddParameters("param1", "param2")
                    .AddValueMapper1(x => MapResult.Success(x.Values.First() + "mapper1"), instanceAppliesTo: x => false)
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
                    .WithRequestData("{\"param1\": \"value1\",\"param2\":\"value2\"}")
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
                    .WithRequestData("")
                    .AddParameters("param1", "param2");

            var binder = CreateBinder(requestGraph);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain(null, null, null);
        }

        public JsonBinder CreateBinder(RequestGraph requestGraph)
        {
            return new JsonBinder(requestGraph.GetRouteDescriptor(),
                requestGraph.GetArgumentBinder(),
                requestGraph.GetHttpRequestMessage());
        }
    }
}
