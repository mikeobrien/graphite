using System.Threading.Tasks;
using System.Xml;
using Graphite;
using Graphite.Binding;
using Graphite.Extensions;
using Graphite.Http;
using NUnit.Framework;
using Should;
using Tests.Common;
using XmlReader = Graphite.Readers.XmlReader;

namespace Tests.Unit.Readers
{
    [TestFixture]
    public class XmlReaderTests
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
        public void Should_only_apply_if_the_content_type_is_application_xml(
            [Values(true, false)] bool isXml)
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Post(null))
                    .WithRequestData("<InputModel/>")
                    .WithRequestParameter("request");

            if (isXml)
            {
                requestGraph.WithContentType(MimeTypes.ApplicationXml);
            }

            CreateReader(requestGraph).Applies()
                .ShouldEqual(isXml);
        }

        [Test]
        public async Task Should_read_xml()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Post(null))
                    .WithRequestData("<InputModel><Value>fark</Value></InputModel>")
                    .WithRequestParameter("request")
                    .WithContentType(MimeTypes.ApplicationXml);

            var result = await CreateReader(requestGraph).Read();

            result.ShouldNotBeNull();
            result.ShouldBeType<InputModel>();
            result.CastTo<InputModel>().Value.ShouldEqual("fark");
        }
        
        [TestCase("<InputModel fark", "There is an error in XML document (1, 17).")]
        [TestCase("<InputModel></Value>", "There is an error in XML document (1, 15).")]
        [TestCase("<InputModel><Value><fark/></Value></InputModel>", 
            "There is an error in XML document (1, 21).")]
        public async Task should_return_friendly_error_message_when_malformed(
            string json, string message)
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Post(null))
                .WithRequestData(json)
                .WithRequestParameter("request")
                .WithContentType(MimeTypes.ApplicationJson);

            var exception = await CreateReader(requestGraph).Should()
                .Throw<BadRequestException>(async x => await x.Read());

            exception.Message.ShouldEqual(message);
        }

        private XmlReader CreateReader(RequestGraph requestGraph)
        {
            return new XmlReader(
                requestGraph.GetRouteDescriptor(),
                requestGraph.GetHttpRequestMessage(),
                new Configuration(), new XmlReaderSettings());
        }
    }
}
