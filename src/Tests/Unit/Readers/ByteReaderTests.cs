using System;
using System.Threading.Tasks;
using Graphite.Extensions;
using Graphite.Readers;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Readers
{
    [TestFixture]
    public class ByteReaderTests
    {
        public class Handler
        {
            public void ByteRequest(byte[] bytes) { }

            public void InputBytesRequest(InputBytes inputBytes) { }

            public void NoRequest() { }

            public void NonBytesRequest(int request) { }
        }

        [Test]
        public void Should_only_apply_to_actions_with_a_bytes_request()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.ByteRequest(null))
                .WithRequestParameter("bytes");
            CreateByteReader(requestGraph).Applies().ShouldBeTrue();
        }

        [Test]
        public void Should_only_apply_to_actions_with_an_input_bytes_request()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.InputBytesRequest(null))
                .WithRequestParameter("inputBytes");
            CreateByteReader(requestGraph).Applies().ShouldBeTrue();
        }

        [Test]
        public void Should_not_apply_to_actions_with_a_non_bytes_request()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.NonBytesRequest(0))
                .WithRequestParameter("request");
            CreateByteReader(requestGraph).Applies().ShouldBeFalse();
        }

        [Test]
        public void Should_not_apply_to_actions_with_no_request()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.NoRequest());
            CreateByteReader(requestGraph).Applies().ShouldBeFalse();
        }

        [Test]
        public async Task Should_pass_bytes_to_bytes_request_parameter()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.ByteRequest(null))
                .WithRequestParameter("bytes")
                .WithRequestData("fark");

            var result = await CreateByteReader(requestGraph).Read();

            result.Status.ShouldEqual(ReadStatus.Success);
            result.Value.ShouldBeType<byte[]>();
            result.Value.As<byte[]>().ShouldOnlyContain<byte>(102, 97, 114, 107);
        }

        [TestCase(null, null)]
        [TestCase("fark/farker", "fark.txt")]
        public async Task Should_pass_input_bytes_to_input_bytes_request_parameter(
            string mimeType, string filename)
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.InputBytesRequest(null))
                .WithRequestParameter("inputBytes")
                .WithRequestData("fark");

            if (mimeType.IsNotNullOrEmpty()) requestGraph.WithContentType(mimeType);
            if (filename.IsNotNullOrEmpty()) requestGraph.WithAttachmentFilename(filename);

            var result = await CreateByteReader(requestGraph).Read();

            result.Status.ShouldEqual(ReadStatus.Success);
            result.Value.ShouldBeType<InputBytes>();
            var inputBytes = result.Value.As<InputBytes>();
            inputBytes.Data.ShouldOnlyContain<byte>(102, 97, 114, 107);
            inputBytes.Length.ShouldEqual(4);
            inputBytes.MimeType.ShouldEqual(mimeType);
            inputBytes.Filename.ShouldEqual(filename);
        }

        private ByteReader CreateByteReader(RequestGraph requestGraph)
        {
            return new ByteReader(
                requestGraph.GetRouteDescriptor(),
                requestGraph.GetHttpRequestMessage());
        }
    }
}
