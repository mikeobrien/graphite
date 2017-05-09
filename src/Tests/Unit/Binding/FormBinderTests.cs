using System.Linq;
using System.Threading.Tasks;
using Graphite.Binding;
using Graphite.Extensions;
using Graphite.Http;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Binding
{
    [TestFixture]
    public class FormBinderTests
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
                    .WithRequestData("param1=value1&param2=value2")
                    .WithContentType(MimeTypes.ApplicationFormUrlEncoded)
                    .AddValueMapper1(x => x.Values);

            if (hasParameters)
            {
                requestGraph.AddQuerystringParameter("param1")
                    .AddQuerystringParameter("param2");
            }

            new FormBinder(requestGraph.ValueMappers,
                requestGraph.Configuration).AppliesTo(requestGraph.GetRequestBinderContext())
                .ShouldEqual(hasParameters);
        }

        [Test]
        public void Should_only_apply_if_there_is_not_a_request_defined_for_the_action(
            [Values(true, false)] bool hasRequest)
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, null))
                    .WithRequestData("param1=value1&param2=value2")
                    .AddQuerystringParameter("param1")
                    .AddQuerystringParameter("param2")
                    .WithContentType(MimeTypes.ApplicationFormUrlEncoded)
                    .AddValueMapper1(x => x.Values);

            if (hasRequest)
            {
                requestGraph.WithRequestParameter("request");
            }

            new FormBinder(requestGraph.ValueMappers,
                requestGraph.Configuration).AppliesTo(requestGraph.GetRequestBinderContext())
                .ShouldEqual(!hasRequest);
        }

        [Test]
        public void Should_only_apply_if_the_content_type_is_form_url_encoded(
            [Values(true, false)] bool isFormUrlEncoded)
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, null))
                    .WithRequestData("param1=value1&param2=value2")
                    .AddQuerystringParameter("param1")
                    .AddQuerystringParameter("param2")
                    .AddValueMapper1(x => x.Values);

            if (isFormUrlEncoded)
            {
                requestGraph.WithContentType(MimeTypes.ApplicationFormUrlEncoded);
            }

            new FormBinder(requestGraph.ValueMappers,
                requestGraph.Configuration).AppliesTo(requestGraph.GetRequestBinderContext())
                .ShouldEqual(isFormUrlEncoded);
        }

        [Test]
        public async Task Should_bind_form_values()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, null))
                    .WithRequestData("param1=value1&param2=value2")
                    .AddQuerystringParameter("param1")
                    .AddQuerystringParameter("param2")
                    .AddValueMapper1(x => x.Values.First());

            var binder = new FormBinder(requestGraph.ValueMappers, 
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
                    .WithRequestData("param1=value1&param1=value2")
                    .AddQuerystringParameter("param1")
                    .AddValueMapper1(x => x.Values);

            var binder = new FormBinder(requestGraph.ValueMappers,
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
                    .WithRequestData("param1=value1")
                    .AddQuerystringParameter("param1")
                    .AddValueMapper1(x => x.Values.First());

            var binder = new FormBinder(requestGraph.ValueMappers,
                requestGraph.Configuration);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain(null, "value1", null);
        }

        [Test]
        public async Task Should_use_the_first_mapper_that_applies()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, null))
                    .WithRequestData("param1=value1&param2=value2")
                    .AddQuerystringParameter("param1")
                    .AddQuerystringParameter("param2")
                    .AddValueMapper1(x => x.Values.First() + "mapper1")
                    .AddValueMapper2(x => x.Values.First() + "mapper2");

            var binder = new FormBinder(requestGraph.ValueMappers,
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
                    .WithRequestData("param1=value1&param2=value2")
                    .AddQuerystringParameter("param1")
                    .AddQuerystringParameter("param2")
                    .AddValueMapper1(x => x.Values.First() + "mapper1", configAppliesTo: x => false)
                    .AddValueMapper2(x => x.Values.First() + "mapper2");

            var binder = new FormBinder(requestGraph.ValueMappers,
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
                    .WithRequestData("param1=value1&param2=value2")
                    .AddQuerystringParameter("param1")
                    .AddQuerystringParameter("param2")
                    .AddValueMapper1(x => x.Values.First() + "mapper1", instanceAppliesTo: x => false)
                    .AddValueMapper2(x => x.Values.First() + "mapper2");

            var binder = new FormBinder(requestGraph.ValueMappers,
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
                    .WithRequestData("param1=value1&param2=value2")
                    .AddQuerystringParameter("param1")
                    .AddQuerystringParameter("param2");

            var binder = new FormBinder(requestGraph.ValueMappers,
                requestGraph.Configuration);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain(null, null, null);
        }

        [Test]
        public async Task Should_map_null_if_there_are_no_parameters()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Params(null, null, null))
                    .WithRequestData("")
                    .AddQuerystringParameter("param1")
                    .AddQuerystringParameter("param2");

            var binder = new FormBinder(requestGraph.ValueMappers,
                requestGraph.Configuration);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain(null, null, null);
        }
    }
}
