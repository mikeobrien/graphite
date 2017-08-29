using System;
using System.Threading.Tasks;
using Graphite.Extensions;
using Graphite.Http;
using Graphite.Readers;
using Newtonsoft.Json;
using NUnit.Framework;
using Should;
using Tests.Common;
using JsonReader = Graphite.Readers.JsonReader;

namespace Tests.Unit.Readers
{
    [TestFixture]
    public class JsonReaderTests
    {
        public class InputModel
        {
            public string Value { get; set; }
        }

        public class Handler
        {
            public void Post(InputModel request) { }
        }

        [Test]
        public void Should_only_apply_if_the_content_type_is_application_json(
            [Values(true, false)] bool isJson)
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Post(null))
                    .WithRequestData("{}")
                    .WithRequestParameter("request");

            if (isJson)
            {
                requestGraph.WithContentType(MimeTypes.ApplicationJson);
            }

            CreateReader(requestGraph)
                .Applies()
                .ShouldEqual(isJson);
        }

        [Test]
        public async Task Should_read_json()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Post(null))
                    .WithRequestData("{\"Value\":\"fark\"}")
                    .WithRequestParameter("request")
                    .WithContentType(MimeTypes.ApplicationJson);

            var result = await CreateReader(requestGraph).Read();

            result.Status.ShouldEqual(ReadStatus.Success);
            result.Value.ShouldNotBeNull();
            result.Value.ShouldBeType<InputModel>();
            result.Value.CastTo<InputModel>().Value.ShouldEqual("fark");
        }

        //JsonReaderException
        [TestCase("{ \"sdfsdf\" }", "Invalid character after parsing property name. " +
                                    "Expected ':' but got: }. Path '', line 1, position 11.")]
        // JsonSerializationException
        [TestCase("{ \"sdfsdf\": { }", "Unexpected end when deserializing object. " +
                                       "Path 'sdfsdf', line 1, position 15.")]
        public async Task should_return_friendly_error_message_when_malformed(
            string json, string message)
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Post(null))
                .WithRequestData(json)
                .WithRequestParameter("request")
                .WithContentType(MimeTypes.ApplicationJson);

            var result = await CreateReader(requestGraph).Read();

            result.Status.ShouldEqual(ReadStatus.Failure);
            result.ErrorMessage.ShouldEqual(message);
        }

        private JsonReader CreateReader(RequestGraph requestGraph)
        {
            return new JsonReader(
                new JsonSerializer(),
                requestGraph.GetRouteDescriptor(),
                requestGraph.GetHttpRequestMessage());
        }
    }
}
