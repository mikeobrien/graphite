using System.Collections.Generic;
using System.IO;
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
                    .AddParameters("param");
            requestGraph.AppendValueMapper(new SimpleTypeMapper(new ParsedValueMapper()));

            if (isForm)
            {
                requestGraph.WithContentType(MimeTypes.ApplicationFormUrlEncoded);
            }

            CreateReader(requestGraph)
                .AppliesTo(CreateReaderContext(requestGraph))
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
                    .AddParameters("param");
            requestGraph.AppendValueMapper(new SimpleTypeMapper(new ParsedValueMapper()));

            var result = await CreateReader(requestGraph)
                .Read(CreateReaderContext(requestGraph));

            result.Status.ShouldEqual(ReadStatus.Success);
            result.Value.ShouldNotBeNull();
            result.Value.ShouldBeType<InputModel>();
            var inputModel = result.Value.CastTo<InputModel>();
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
                    .AddParameters("param");
            requestGraph.AppendValueMapper(new SimpleTypeMapper(new ParsedValueMapper()));

            var result = await CreateReader(requestGraph)
                .Read(CreateReaderContext(requestGraph));

            result.Status.ShouldEqual(ReadStatus.Success);
            result.Value.ShouldNotBeNull();
            result.Value.ShouldBeType<InputModel>();
            var inputModel = result.Value.CastTo<InputModel>();
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
                    .AddParameters("param");
            requestGraph.AppendValueMapper(new SimpleTypeMapper(new ParsedValueMapper()));

            var result = await CreateReader(requestGraph)
                .Read(CreateReaderContext(requestGraph));

            result.Status.ShouldEqual(ReadStatus.Success);
            result.Value.ShouldNotBeNull();
            result.Value.ShouldBeType<MultiInputModel>();
            var inputModel = result.Value.CastTo<MultiInputModel>();
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
                    .AddValueMapper1(x => MapResult.Success(x.Values.First() + "mapper1"))
                    .AddValueMapper2(x => MapResult.Success(x.Values.First() + "mapper2"));

            var reader = CreateReader(requestGraph);

            var result = await reader
                .Read(CreateReaderContext(requestGraph));

            result.Status.ShouldEqual(ReadStatus.Success);
            result.Value.ShouldNotBeNull();
            result.Value.ShouldBeType<InputModel>();
            var inputModel = result.Value.CastTo<InputModel>();
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
                    .AddValueMapper1(x => MapResult.Success(x.Values.First() + "mapper1"), configAppliesTo: x => false)
                    .AddValueMapper2(x => MapResult.Success(x.Values.First() + "mapper2"));

            var reader = CreateReader(requestGraph);

            var result = await reader
                .Read(CreateReaderContext(requestGraph));

            result.Status.ShouldEqual(ReadStatus.Success);
            result.Value.ShouldNotBeNull();
            result.Value.ShouldBeType<InputModel>();
            var inputModel = result.Value.CastTo<InputModel>();
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
                    .AddValueMapper1(x => MapResult.Success(x.Values.First() + "mapper1"), instanceAppliesTo: x => false)
                    .AddValueMapper2(x => MapResult.Success(x.Values.First() + "mapper2"));

            var reader = CreateReader(requestGraph);

            var result = await reader
                .Read(CreateReaderContext(requestGraph));

            result.Status.ShouldEqual(ReadStatus.Success);
            result.Value.ShouldNotBeNull();
            result.Value.ShouldBeType<InputModel>();
            var inputModel = result.Value.CastTo<InputModel>();
            inputModel.Param1.ShouldEqual("value1mapper2");
            inputModel.Param2.ShouldEqual(0);

            requestGraph.ValueMapper1.AppliesToCalled.ShouldBeTrue();
            requestGraph.ValueMapper1.MapCalled.ShouldBeFalse();

            requestGraph.ValueMapper2.AppliesToCalled.ShouldBeTrue();
            requestGraph.ValueMapper2.MapCalled.ShouldBeTrue();
        }

        [Test]
        public async Task Should_skip_mapping_if_no_mappers_apply()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Post(null, null))
                    .WithRequestData("param1=value1")
                    .WithRequestParameter("request");

            var reader = CreateReader(requestGraph);

            var result = await reader
                .Read(CreateReaderContext(requestGraph));

            result.Status.ShouldEqual(ReadStatus.Success);
            result.Value.ShouldNotBeNull();
            result.Value.ShouldBeType<InputModel>();
            var inputModel = result.Value.CastTo<InputModel>();
            inputModel.Param1.ShouldBeNull();
            inputModel.Param2.ShouldEqual(0);
        }

        [Test]
        public async Task Should_throw_exception_if_configured_no_mappers_apply()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Post(null, null))
                    .WithRequestData("param1=value1")
                    .WithRequestParameter("request");

            requestGraph.Configuration.FailIfNoMapperFound = true;

            var reader = CreateReader(requestGraph);

            var exception = await reader.Should().Throw<MapperNotFoundException>(x => x
                .Read(CreateReaderContext(requestGraph)));

            exception.Message.ShouldEqual("Unable to map 'value1' to type string for 'Param1' " +
                "parameter on action Tests.Unit.Readers.FormReaderTests.Handler.Post.");
        }

        [Test]
        public async Task Should_return_failure_result_if_mapping_fails()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Post(null, null))
                .WithRequestData("Param2=fark")
                .WithRequestParameter("request")
                .WithContentType(MimeTypes.ApplicationFormUrlEncoded);
            requestGraph.AppendValueMapper(new SimpleTypeMapper(new ParsedValueMapper()));

            var result = await CreateReader(requestGraph)
                .Read(CreateReaderContext(requestGraph));

            result.Status.ShouldEqual(ReadStatus.Failure);
            result.ErrorMessage.ShouldEqual(
                "Property 'Param2' value 'fark' is not formatted correctly. " +
                "'fark' is not a valid 32 bit integer. Must be an integer between -2,147,483,648 and 2,147,483,647.");
        }

        public class CtorInputModel
        {
            public CtorInputModel(string fark) { }
        }

        public class CtorHandler
        {
            public void Post(CtorInputModel request) { }
        }

        [Test]
        public async Task Should_throw_exception_if_parameter_type_does_not_have_a_parameterless_ctor()
        {
            var requestGraph = RequestGraph
                .CreateFor<CtorHandler>(h => h.Post(null))
                .WithRequestData("param=value")
                .WithRequestParameter("request");

            var reader = CreateReader(requestGraph);

            var exception = await reader.Should().Throw<RequestTypeCreationException>(x => x
                .Read(CreateReaderContext(requestGraph)));

            exception.Message.ShouldEqual("Unable to instantiate type Tests.Unit.Readers.FormReaderTests" +
                ".CtorInputModel for action Tests.Unit.Readers.FormReaderTests" +
                ".CtorHandler.Post. Type must have a parameterless constructor.");
        }

        private ReaderContext CreateReaderContext(RequestGraph requestGraph)
        {
            return new ReaderContext(
                requestGraph.RequestParameter?.ParameterType, 
                requestGraph.ContentType, null,
                requestGraph.AttachmentFilename,
                requestGraph.GetHttpHeaders(),
                requestGraph.RequestData == null ? null :
                    new MemoryStream(requestGraph.RequestData)
                        .ToTaskResult<Stream>(),
                contentLength: requestGraph.RequestData?.Length);
        }

        private FormReader CreateReader(RequestGraph requestGraph)
        {
            var parameterBinder = new ParameterBinder<ReadResult>(
                requestGraph.Configuration,
                requestGraph.HttpConfiguration,
                requestGraph.ActionMethod,
                requestGraph.GetRouteDescriptor(),
                requestGraph.ValueMappers);
            return new FormReader(parameterBinder, requestGraph.ActionMethod);
        }
    }
}
