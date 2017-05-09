using System;
using System.IO;
using System.Threading.Tasks;
using Graphite;
using Graphite.Extensions;
using Graphite.Readers;
using Graphite.Writers;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Readers
{
    [TestFixture]
    public class StreamReaderTests
    {
        public class Handler
        {
            public void StreamRequest(Stream stream) { }

            public void InputStreamRequest(InputStream inputStream) { }

            public void NoRequest() { }

            public void NonStreamRequest(int request) { }
        }

        [Test]
        public void Should_only_apply_to_actions_with_a_stream_request()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.StreamRequest(null))
                .WithRequestParameter("stream");
            new Graphite.Readers.StreamReader().AppliesTo(requestGraph
                .GetRequestReaderContext()).ShouldBeTrue();
        }

        [Test]
        public void Should_only_apply_to_actions_with_an_input_stream_request()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.InputStreamRequest(null))
                .WithRequestParameter("inputStream");
            new Graphite.Readers.StreamReader().AppliesTo(requestGraph
                .GetRequestReaderContext()).ShouldBeTrue();
        }

        [Test]
        public void Should_not_apply_to_actions_with_a_non_stream_request()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.NonStreamRequest(0))
                .WithRequestParameter("request");
            new Graphite.Readers.StreamReader().AppliesTo(requestGraph
                .GetRequestReaderContext()).ShouldBeFalse();
        }

        [Test]
        public void Should_not_apply_to_actions_with_no_request()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.NoRequest());
            new Graphite.Readers.StreamReader().AppliesTo(requestGraph
                .GetRequestReaderContext()).ShouldBeFalse();
        }

        [Test]
        public async Task Should_pass_stream_to_stream_request_parameter()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.StreamRequest(null))
                .WithRequestParameter("stream")
                .WithRequestData("fark");

            var result = await new Graphite.Readers.StreamReader().Read(requestGraph
                .GetRequestReaderContext());

            result.ShouldBeType<MemoryStream>();
            result.As<Stream>().ReadAllText().ShouldEqual("fark");
        }

        [TestCase(null, null)]
        [TestCase("fark/farker", "fark.txt")]
        public async Task Should_pass_input_stream_to_input_stream_request_parameter(
            string mimeType, string filename)
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.InputStreamRequest(null))
                .WithRequestParameter("inputStream")
                .WithRequestData("fark");

            if (mimeType.IsNotNullOrEmpty()) requestGraph.WithContentType(mimeType);
            if (filename.IsNotNullOrEmpty()) requestGraph.WithAttachmentFilename(filename);

            var result = await new Graphite.Readers.StreamReader().Read(requestGraph
                .GetRequestReaderContext());

            result.ShouldBeType<InputStream>();
            var inputStream = result.As<InputStream>();
            inputStream.Stream.ReadAllText().ShouldEqual("fark");
            inputStream.Length.ShouldEqual(4);
            inputStream.MimeType.ShouldEqual(mimeType);
            inputStream.Filename.ShouldEqual(filename);
        }
    }
}
