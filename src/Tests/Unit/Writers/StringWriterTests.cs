using System;
using System.Text;
using System.Threading.Tasks;
using Graphite.Http;
using Graphite.Writers;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Writers
{
    [TestFixture]
    public class StringWriterTests
    {
        public class Handler
        {
            public string StringResponse()
            {
                return null;
            }

            [OutputString("fark/farker", "somefile.txt", "utf-8")]
            public string OutputStringAttributeResponse()
            {
                return null;
            }

            public OutputString OutputStringResponse()
            {
                return null;
            }

            public void NoResponse() { }

            public int NonStringResponse()
            {
                return 0;
            }
        }

        [Test]
        public void Should_only_apply_to_actions_returning_string()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.StringResponse());
            CreateStringWriter(requestGraph)
                .AppliesTo(requestGraph
                .GetResponseWriterContext(null)).ShouldBeTrue();
        }

        [Test]
        public void Should_not_apply_to_actions_returning_non_string()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.NonStringResponse());
            CreateStringWriter(requestGraph)
                .AppliesTo(requestGraph
                .GetResponseWriterContext(null)).ShouldBeFalse();
        }

        [Test]
        public void Should_not_apply_to_actions_with_no_return()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.NoResponse());
            CreateStringWriter(requestGraph)
                .AppliesTo(requestGraph
                .GetResponseWriterContext(null)).ShouldBeFalse();
        }

        [TestCase(null, null)]
        [TestCase("", MimeTypes.TextPlain)]
        [TestCase("fark", MimeTypes.TextPlain)]
        [TestCase("<b>fark</b>", MimeTypes.TextHtml)]
        public async Task Should_write_string(string value, string contentType)
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.NoResponse());
            var result = await CreateStringWriter(requestGraph)
                .Write(requestGraph.GetResponseWriterContext(value));

            (result.Content?.ReadAsStringAsync().Result).ShouldEqual(value);
            (result.Content?.Headers.ContentType.MediaType).ShouldEqual(contentType);
        }

        [TestCase("fark", MimeTypes.TextPlain)]
        [TestCase("<b>fark</b>", MimeTypes.TextHtml)]
        [TestCase("fark<br/>", MimeTypes.TextHtml)]
        public async Task Should_return_string_from_string_response(
            string data, string contentType)
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.StringResponse());

            var result = await CreateStringWriter(requestGraph)
                .Write(requestGraph.GetResponseWriterContext(data));

            result.Content.ReadAsStreamAsync().Result.ReadAllText().ShouldEqual(data);
            result.Content.Headers.ContentType.MediaType.ShouldEqual(contentType);
        }

        [Test]
        public async Task Should_return_string_from_attribute_configured_response()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.OutputStringAttributeResponse());

            var result = await CreateStringWriter(requestGraph)
                .Write(requestGraph.GetResponseWriterContext("fark"));

            (result.Content?.ReadAsStreamAsync().Result.ReadAllText()).ShouldEqual("fark");

            var headers = result.Content?.Headers;

            (headers?.ContentType?.MediaType).ShouldEqual("fark/farker");
            (headers?.ContentDisposition?.FileName).ShouldEqual("somefile.txt");
        }

        [TestCase(null, null, null, null, null, null)]
        [TestCase("", "", null, MimeTypes.TextPlain, null, null)]
        [TestCase("fark", "fark", null, MimeTypes.TextPlain, null, null)]
        [TestCase("<b>fark</b>", "<b>fark</b>", null, MimeTypes.TextHtml, null, null)]
        [TestCase("fark<br/>", "fark<br/>", null, MimeTypes.TextHtml, null, null)]
        [TestCase("fark", "fark", MimeTypes.TextHtml, MimeTypes.TextHtml, null, null)]
        [TestCase("Ӝark", "Ӝark", null, MimeTypes.TextPlain, null, null)]
        [TestCase("Ӝark", "?ark", null, MimeTypes.TextPlain, null, "ascii")]
        [TestCase("fark", "fark", null, MimeTypes.TextPlain, "fark.txt", null)]
        public async Task Should_return_string_from_output_string_response(
            string data, string expectedData, 
            string contentType, string expectedContentType, 
            string filename, string encoding)
        {
            var outputString = new OutputString
            {
                Data = data,
                ContentType = contentType,
                Filename = filename,
                Encoding = encoding == null ? null : Encoding.GetEncoding(encoding)
            };
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.OutputStringResponse());

            var result = await CreateStringWriter(requestGraph)
                .Write(requestGraph.GetResponseWriterContext(outputString));
            
            (result.Content?.ReadAsStreamAsync().Result.ReadAllText()).ShouldEqual(expectedData);

            var headers = result.Content?.Headers;

            (headers?.ContentType?.MediaType).ShouldEqual(expectedContentType);
            (headers?.ContentDisposition?.FileName).ShouldEqual(filename);
        }

        private StringWriter CreateStringWriter(RequestGraph requestGraph)
        {
            return new StringWriter(
                requestGraph.ActionMethod,
                requestGraph.GetRouteDescriptor(),
                requestGraph.GetHttpResponseMessage());
        }
    }
}
