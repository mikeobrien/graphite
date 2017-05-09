using System.IO;
using System.Text;
using System.Threading.Tasks;
using Graphite;
using Graphite.Http;
using Graphite.Writers;
using NUnit.Framework;
using Should;
using Tests.Common;

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
            new Graphite.Writers.StreamWriter(new Configuration()).AppliesTo(requestGraph
                .GetResponseWriterContext(null)).ShouldBeTrue();
        }

        [Test]
        public void Should_only_apply_to_actions_returning_output_stream()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.StreamResponse());
            new Graphite.Writers.StreamWriter(new Configuration()).AppliesTo(requestGraph
                .GetResponseWriterContext(null)).ShouldBeTrue();
        }

        [Test]
        public void Should_not_apply_to_actions_returning_non_string()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.NonStreamResponse());
            new Graphite.Writers.StreamWriter(new Configuration()).AppliesTo(requestGraph
                .GetResponseWriterContext(null)).ShouldBeFalse();
        }

        [Test]
        public void Should_not_apply_to_actions_returning_nothing()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.NoResponse());
            new Graphite.Writers.StreamWriter(new Configuration()).AppliesTo(requestGraph
                .GetResponseWriterContext(null)).ShouldBeFalse();
        }

        [Test]
        public async Task Should_return_stream_from_stream_response()
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("fark"));
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.StreamResponse());

            var result = await new Graphite.Writers.StreamWriter(new Configuration()).Write(requestGraph
                .GetResponseWriterContext(stream));

            result.Content.ReadAsStreamAsync().Result.ReadAllText().ShouldEqual("fark");
            result.Content.Headers.ContentType.MediaType.ShouldEqual(MimeTypes.ApplicationOctetStream);
        }

        [TestCase(MimeTypes.ApplicationOctetStream, null)]
        [TestCase("fark/farker", "fark.txt")]
        public async Task Should_return_stream_from_output_stream_response(
            string contentType, string filename)
        {
            var outputStream = new OutputStream
            {
                Stream = new MemoryStream(Encoding.UTF8.GetBytes("fark")),
                ContentType = contentType,
                Filename = filename
            };
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.OutputStreamResponse());

            var result = await new Graphite.Writers.StreamWriter(new Configuration()).Write(requestGraph
                .GetResponseWriterContext(outputStream));

            result.Content.ReadAsStreamAsync().Result.ReadAllText().ShouldEqual("fark");
            
            (result.Content.Headers.ContentType?.MediaType).ShouldEqual(contentType);
            (result.Content.Headers.ContentDisposition?.FileName).ShouldEqual(filename);
        }
    }
}
