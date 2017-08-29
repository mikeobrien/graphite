using System;
using System.IO;
using System.Threading.Tasks;
using Graphite.Extensions;
using Graphite.Readers;
using NUnit.Framework;
using Should;
using Tests.Common;
using StringReader = Graphite.Readers.StringReader;

namespace Tests.Unit.Readers
{
    [TestFixture]
    public class StringReaderTests
    {
        public class Handler
        {
            public void StringRequest(string value) { }

            public void InputStringRequest(InputString inputString) { }

            public void NoRequest() { }

            public void NonStringRequest(int request) { }
        }

        [Test]
        public void Should_only_apply_to_actions_with_a_string_request()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.StringRequest(null))
                .WithRequestParameter("value");
            CreateStringReader(requestGraph).Applies().ShouldBeTrue();
        }

        [Test]
        public void Should_only_apply_to_actions_with_an_input_string_request()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.InputStringRequest(null))
                .WithRequestParameter("inputString");
            CreateStringReader(requestGraph).Applies().ShouldBeTrue();
        }

        [Test]
        public void Should_not_apply_to_actions_with_a_non_string_request()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.NonStringRequest(0))
                .WithRequestParameter("request");
            CreateStringReader(requestGraph).Applies().ShouldBeFalse();
        }

        [Test]
        public void Should_not_apply_to_actions_with_no_request()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.NoRequest());
            CreateStringReader(requestGraph).Applies().ShouldBeFalse();
        }

        [Test]
        public async Task Should_pass_string_to_string_request_parameter()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.StringRequest(null))
                .WithRequestParameter("value")
                .WithRequestData("fark");

            var result = await CreateStringReader(requestGraph).Read();

            result.Status.ShouldEqual(ReadStatus.Success);
            result.Value.ShouldBeType<string>();
            result.Value.As<string>().ShouldEqual("fark");
        }

        [TestCase(null, null)]
        [TestCase("fark/farker", "fark.txt")]
        public async Task Should_pass_input_string_to_input_string_request_parameter(
            string mimeType, string filename)
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.InputStringRequest(null))
                .WithRequestParameter("inputString")
                .WithRequestData("fark");

            if (mimeType.IsNotNullOrEmpty()) requestGraph.WithContentType(mimeType);
            if (filename.IsNotNullOrEmpty()) requestGraph.WithAttachmentFilename(filename);

            var result = await CreateStringReader(requestGraph).Read();

            result.Status.ShouldEqual(ReadStatus.Success);
            result.Value.ShouldBeType<InputString>();
            var inputString = result.Value.As<InputString>();
            inputString.Data.ShouldEqual("fark");
            inputString.Length.ShouldEqual(4);
            inputString.MimeType.ShouldEqual(mimeType);
            inputString.Filename.ShouldEqual(filename);
        }

        private StringReader CreateStringReader(RequestGraph requestGraph)
        {
            return new StringReader(
                requestGraph.GetRouteDescriptor(),
                requestGraph.GetHttpRequestMessage());
        }
    }
}
