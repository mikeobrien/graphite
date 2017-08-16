using System;
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
    public class ByteWriterTests
    {
        public class Handler
        {
            public byte[] ByteResponse()
            {
                return null;
            }

            [OutputBytes("fark/farker", "somefile.txt")]
            public byte[] OutputBytesAttributeResponse()
            {
                return null;
            }

            public OutputBytes OutputBytesResponse()
            {
                return null;
            }

            public void NoResponse() { }

            public int NonByteResponse()
            {
                return 0;
            }
        }

        [Test]
        public void Should_only_apply_to_actions_returning_bytes()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.ByteResponse());
            CreateByteWriter(requestGraph)
                .AppliesTo(requestGraph.GetResponseWriterContext(null)).ShouldBeTrue();
        }

        [Test]
        public void Should_only_apply_to_actions_returning_output_bytes()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.ByteResponse());
            CreateByteWriter(requestGraph)
                .AppliesTo(requestGraph.GetResponseWriterContext(null)).ShouldBeTrue();
        }

        [Test]
        public void Should_not_apply_to_actions_returning_non_bytes()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.NonByteResponse());
            CreateByteWriter(requestGraph)
                .AppliesTo(requestGraph.GetResponseWriterContext(null)).ShouldBeFalse();
        }

        [Test]
        public void Should_not_apply_to_actions_returning_nothing()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.NoResponse());
            CreateByteWriter(requestGraph)
                .AppliesTo(requestGraph.GetResponseWriterContext(null)).ShouldBeFalse();
        }

        [Test]
        public async Task Should_return_bytes_from_bytes_response()
        {
            var bytes = Encoding.UTF8.GetBytes("fark");
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.ByteResponse());

            var result = await CreateByteWriter(requestGraph)
                .Write(requestGraph.GetResponseWriterContext(bytes));

            result.Content.ReadAsStreamAsync().Result.ReadAllText().ShouldEqual("fark");
            result.Content.Headers.ContentType.MediaType.ShouldEqual(MimeTypes.ApplicationOctetStream);
        }

        [Test]
        public async Task Should_return_bytes_from_attribute_configured_response()
        {
            var bytes = Encoding.UTF8.GetBytes("fark");
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.OutputBytesAttributeResponse());

            var result = await CreateByteWriter(requestGraph)
                .Write(requestGraph.GetResponseWriterContext(bytes));

            (result.Content?.ReadAsStreamAsync().Result.ReadAllText()).ShouldEqual("fark");

            var headers = result.Content?.Headers;

            (headers?.ContentType?.MediaType).ShouldEqual("fark/farker");
            (headers?.ContentDisposition?.FileName).ShouldEqual("somefile.txt");
        }

        [TestCase(null, MimeTypes.ApplicationOctetStream, null)]
        [TestCase(MimeTypes.ApplicationOctetStream, MimeTypes.ApplicationOctetStream, null)]
        [TestCase("fark/farker", "fark/farker", "fark.txt")]
        public async Task Should_return_bytes_from_output_bytes_response(
            string contentType, string expectedContentType, string filename)
        {
            var outputStream = new OutputBytes
            {
                Data = Encoding.UTF8.GetBytes("fark"),
                ContentType = contentType,
                Filename = filename
            };
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.OutputBytesResponse());

            var result = await CreateByteWriter(requestGraph)
                .Write(requestGraph.GetResponseWriterContext(outputStream));

            result.Content.ReadAsStreamAsync().Result.ReadAllText().ShouldEqual("fark");

            (result.Content.Headers.ContentType?.MediaType).ShouldEqual(expectedContentType);
            (result.Content.Headers.ContentDisposition?.FileName).ShouldEqual(filename);
        }

        private ByteWriter CreateByteWriter(RequestGraph requestGraph)
        {
            return new ByteWriter(
                requestGraph.ActionMethod,
                requestGraph.GetRouteDescriptor(),
                requestGraph.GetHttpResponseMessage(),
                new Configuration());
        }
    }
}
