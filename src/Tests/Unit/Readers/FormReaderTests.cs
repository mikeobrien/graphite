using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graphite.Binding;
using Graphite.Extensions;
using Graphite.Http;
using Graphite.Readers;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Readers
{
    [TestFixture]
    public class FormReaderTests
    {
        public class InputModel
        {
            public string Param1 { get; set; }
            public int Param2 { get; set; }
        }

        public class Handler
        {
            public void Post(InputModel request, string param) { }
        }

        [Test]
        public void Should_only_apply_if_the_content_type_is_form_url_encoded(
            [Values(true, false)] bool isForm)
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Post(null, null))
                    .WithRequestData("Param1=value1&Param2=value2")
                    .WithRequestParameter("request")
                    .AddParameters("param")
                    .AppendValueMapper<SimpleTypeMapper>();

            if (isForm)
            {
                requestGraph.WithContentType(MimeTypes.ApplicationFormUrlEncoded);
            }

            CreateReader(requestGraph)
                .Applies()
                .ShouldEqual(isForm);
        }

        [Test]
        public async Task Should_read_form()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Post(null, null))
                    .WithRequestData("Param1=value1&Param2=5")
                    .WithRequestParameter("request")
                    .WithContentType(MimeTypes.ApplicationFormUrlEncoded)
                    .AddParameters("param")
                    .AppendValueMapper<SimpleTypeMapper>();

            var result = await CreateReader(requestGraph).Read();

            result.ShouldNotBeNull();
            result.ShouldBeType<InputModel>();
            var inputModel = result.CastTo<InputModel>();
            inputModel.Param1.ShouldEqual("value1");
            inputModel.Param2.ShouldEqual(5);
        }

        [Test]
        public async Task Should_only_map_the_first_value_if_the_destination_type_is_not_an_array_or_list()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Post(null, null))
                    .WithRequestData("Param1=value1&Param1=value2&Param2=5&Param2=6")
                    .WithRequestParameter("request")
                    .WithContentType(MimeTypes.ApplicationFormUrlEncoded)
                    .AddParameters("param")
                    .AppendValueMapper<SimpleTypeMapper>();

            var result = await CreateReader(requestGraph).Read();

            result.ShouldNotBeNull();
            result.ShouldBeType<InputModel>();
            var inputModel = result.CastTo<InputModel>();
            inputModel.Param1.ShouldEqual("value1");
            inputModel.Param2.ShouldEqual(5);
        }

        public class MultiInputModel
        {
            public string[] ParamArray { get; set; }
            public List<int> ParamList { get; set; }
        }

        public class MultiHandler
        {
            public void Post(MultiInputModel request, string param) { }
        }

        [Test]
        public async Task Should_read_form_with_multiple_values_with_the_same_name()
        {
            var requestGraph = RequestGraph
                .CreateFor<MultiHandler>(h => h.Post(null, null))
                    .WithRequestData("ParamArray=value1&ParamArray=value2&ParamList=3&ParamList=4")
                    .WithRequestParameter("request")
                    .WithContentType(MimeTypes.ApplicationFormUrlEncoded)
                    .AddParameters("param")
                    .AppendValueMapper<SimpleTypeMapper>();

            var result = await CreateReader(requestGraph).Read();

            result.ShouldNotBeNull();
            result.ShouldBeType<MultiInputModel>();
            var inputModel = result.CastTo<MultiInputModel>();
            inputModel.ParamArray.ShouldOnlyContain("value1", "value2");
            inputModel.ParamList.ShouldOnlyContain(3, 4);
        }

        [Test]
        public async Task Should_use_the_first_mapper_that_applies()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Post(null, null))
                    .WithRequestData("param1=value1")
                    .WithRequestParameter("request")
                    .AddValueMapper1(x => x.Values.First() + "mapper1")
                    .AddValueMapper2(x => x.Values.First() + "mapper2");

            var reader = CreateReader(requestGraph);

            var result = await reader.Read();

            result.ShouldNotBeNull();
            result.ShouldBeType<InputModel>();
            var inputModel = result.CastTo<InputModel>();
            inputModel.Param1.ShouldEqual("value1mapper1");
            inputModel.Param2.ShouldEqual(0);

            requestGraph.ValueMapper1.AppliesToCalled.ShouldBeTrue();
            requestGraph.ValueMapper1.MapCalled.ShouldBeTrue();

            requestGraph.ValueMapper2.AppliesToCalled.ShouldBeTrue();
            requestGraph.ValueMapper2.MapCalled.ShouldBeFalse();
        }

        [Test]
        public async Task Should_not_use_a_mapper_that_doesnt_apply_in_configuration()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Post(null, null))
                    .WithRequestData("param1=value1")
                    .WithRequestParameter("request")
                    .AddValueMapper1(x => x.Values.First() + "mapper1", configAppliesTo: x => false)
                    .AddValueMapper2(x => x.Values.First() + "mapper2");

            var reader = CreateReader(requestGraph);

            var result = await reader.Read();

            result.ShouldNotBeNull();
            result.ShouldBeType<InputModel>();
            var inputModel = result.CastTo<InputModel>();
            inputModel.Param1.ShouldEqual("value1mapper2");
            inputModel.Param2.ShouldEqual(0);

            requestGraph.ValueMapper1.AppliesToCalled.ShouldBeFalse();
            requestGraph.ValueMapper1.MapCalled.ShouldBeFalse();

            requestGraph.ValueMapper2.AppliesToCalled.ShouldBeTrue();
            requestGraph.ValueMapper2.MapCalled.ShouldBeTrue();
        }

        [Test]
        public async Task Should_not_use_a_mapper_that_doesnt_apply_at_runtime()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Post(null, null))
                    .WithRequestData("param1=value1")
                    .WithRequestParameter("request")
                    .AddValueMapper1(x => x.Values.First() + "mapper1", instanceAppliesTo: x => false)
                    .AddValueMapper2(x => x.Values.First() + "mapper2");

            var reader = CreateReader(requestGraph);

            var result = await reader.Read();

            result.ShouldNotBeNull();
            result.ShouldBeType<InputModel>();
            var inputModel = result.CastTo<InputModel>();
            inputModel.Param1.ShouldEqual("value1mapper2");
            inputModel.Param2.ShouldEqual(0);

            requestGraph.ValueMapper1.AppliesToCalled.ShouldBeTrue();
            requestGraph.ValueMapper1.MapCalled.ShouldBeFalse();

            requestGraph.ValueMapper2.AppliesToCalled.ShouldBeTrue();
            requestGraph.ValueMapper2.MapCalled.ShouldBeTrue();
        }

        [Test]
        public async Task Should_bind_the_original_value_if_no_mappers_apply()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Post(null, null))
                    .WithRequestData("param1=value1")
                    .WithRequestParameter("request");

            var reader = CreateReader(requestGraph);

            var result = await reader.Read();

            result.ShouldNotBeNull();
            result.ShouldBeType<InputModel>();
            var inputModel = result.CastTo<InputModel>();
            inputModel.Param1.ShouldBeNull();
            inputModel.Param2.ShouldEqual(0);
        }

        [Test]
        public async Task Should_map_null_if_there_are_no_parameters()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Post(null, null))
                    .WithRequestData("")
                    .WithRequestParameter("request");

            var reader = CreateReader(requestGraph);

            var result = await reader.Read();

            result.ShouldNotBeNull();
            result.ShouldBeType<InputModel>();
            var inputModel = result.CastTo<InputModel>();
            inputModel.Param1.ShouldBeNull();
            inputModel.Param2.ShouldEqual(0);
        }

        private FormReader CreateReader(RequestGraph requestGraph)
        {
            return new FormReader(
                requestGraph.Configuration,
                requestGraph.HttpConfiguration,
                requestGraph.ActionMethod,
                requestGraph.GetRouteDescriptor(),
                requestGraph.ValueMappers,
                requestGraph.GetHttpRequestMessage());
        }
    }
}
