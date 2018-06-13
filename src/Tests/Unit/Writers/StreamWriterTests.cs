using System.IO;
using System.Text;
using System.Threading.Tasks;
using Graphite;
using Graphite.Extensions;
using Graphite.Http;
using Graphite.Writers;
using NUnit.Framework;
using Should;
using Tests.Common;
using StreamWriter = Graphite.Writers.StreamWriter;

namespace Tests.Unit.Writers
{
    [TestFixture]
    public class StreamWriterTests
    {
        public class Handler
        {
            public Stream StreamResponse()
            {
                return null;
            }

            [OutputStream("fark/farker", "somefile.txt", 10)]
            public Stream OutputStreamAttributeResponse()
            {
                return null;
            }

            public OutputStream OutputStreamResponse()
            {
                return null;
            }

            public void NoResponse() { }

            public int NonStreamResponse()
            {
                return 0;
            }
        }

        [Test]
        public void Should_only_apply_to_actions_returning_stream()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.StreamResponse());
            CreateStreamWriter(requestGraph)
                .AppliesTo(requestGraph.GetResponseWriterContext(null)).ShouldBeTrue();
        }

        [Test]
        public void Should_only_apply_to_actions_returning_output_stream()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.StreamResponse());
            CreateStreamWriter(requestGraph)
                .AppliesTo(requestGraph.GetResponseWriterContext(null)).ShouldBeTrue();
        }

        [Test]
        public void Should_not_apply_to_actions_returning_non_string()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.NonStreamResponse());
            CreateStreamWriter(requestGraph)
                .AppliesTo(requestGraph.GetResponseWriterContext(null)).ShouldBeFalse();
        }

        [Test]
        public void Should_not_apply_to_actions_returning_nothing()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.NoResponse());
            CreateStreamWriter(requestGraph)
                .AppliesTo(requestGraph.GetResponseWriterContext(null)).ShouldBeFalse();
        }

        [Test]
        public async Task Should_return_stream_from_stream_response()
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("fark"));
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.StreamResponse());

            var result = await CreateStreamWriter(requestGraph)
                .Write(requestGraph.GetResponseWriterContext(stream));

            result.Content.ReadAsStreamAsync().Result.ReadAllText().ShouldEqual("fark");
            result.Content.Headers.ContentType.MediaType.ShouldEqual(MimeTypes.ApplicationOctetStream);
        }

        [Test]
        public async Task Should_return_stream_from_attribute_configured_response()
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("fark"));
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.OutputStreamAttributeResponse());

            var result = await CreateStreamWriter(requestGraph)
                .Write(requestGraph.GetResponseWriterContext(stream));

            (result.Content?.ReadAsStreamAsync().Result.ReadAllText()).ShouldEqual("fark");

            var headers = result.Content?.Headers;

            (headers?.ContentType?.MediaType).ShouldEqual("fark/farker");
            (headers?.ContentDisposition?.FileName).ShouldEqual("somefile.txt");
        }

        [TestCase(null, MimeTypes.ApplicationOctetStream, null, null, AttachmentFilenameQuoting.Default)]
        [TestCase(MimeTypes.ApplicationOctetStream, MimeTypes.ApplicationOctetStream, null, null, AttachmentFilenameQuoting.Default)]
        [TestCase("fark/farker", "fark/farker", "fark.txt", "fark.txt", AttachmentFilenameQuoting.Default)]
        [TestCase("fark/farker", "fark/farker", "fark\".txt", "fark.txt", AttachmentFilenameQuoting.Default)]
        [TestCase("fark/farker", "fark/farker", "fark.txt", "\"fark.txt\"", AttachmentFilenameQuoting.AlwaysQuote)]
        public async Task Should_return_stream_from_output_stream_response(
            string contentType, string expectedContentType, string filename, 
            string expectedFilename, AttachmentFilenameQuoting quoting)
        {
            var outputStream = new OutputStream
            {
                Data = new MemoryStream(Encoding.UTF8.GetBytes("fark")),
                ContentType = contentType,
                Filename = filename
            };
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.OutputStreamResponse())
                .Configure(x => x.WithAttachmentFilenameQuoting(quoting));

            var result = await CreateStreamWriter(requestGraph)
                .Write(requestGraph.GetResponseWriterContext(outputStream));

            result.Content.ReadAsStreamAsync().Result.ReadAllText().ShouldEqual("fark");
            
            (result.Content.Headers.ContentType?.MediaType).ShouldEqual(expectedContentType);
            (result.Content.Headers.ContentDisposition?.FileName).ShouldEqual(expectedFilename);
        }

        private StreamWriter CreateStreamWriter(RequestGraph requestGraph)
        {
            return new StreamWriter(requestGraph.Configuration,
                requestGraph.ActionMethod, 
                requestGraph.GetRouteDescriptor(), 
                requestGraph.GetHttpResponseMessage());
        }
    }
}
