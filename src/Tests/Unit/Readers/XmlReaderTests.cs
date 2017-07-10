using System.Threading.Tasks;
using Graphite.Extensions;
using Graphite.Http;
using Graphite.Readers;
using NUnit.Framework;
using Should;
using Tests.Common;

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
            public void Post(InputModel request, string param) { }
        }

        [Test]
        public void Should_only_apply_if_the_content_type_is_application_xml(
            [Values(true, false)] bool isXml)
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Post(null, null))
                    .WithRequestData("<InputModel/>")
                    .WithRequestParameter("request")
                    .AddParameters("param");

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
                .CreateFor<Handler>(h => h.Post(null, null))
                    .WithRequestData("<InputModel><Value>fark</Value></InputModel>")
                    .WithRequestParameter("request")
                    .WithContentType(MimeTypes.ApplicationXml)
                    .AddParameters("param");

            var result = await CreateReader(requestGraph).Read();

            result.ShouldNotBeNull();
            result.ShouldBeType<InputModel>();
            result.CastTo<InputModel>().Value.ShouldEqual("fark");
        }

        private XmlReader CreateReader(RequestGraph requestGraph)
        {
            return new XmlReader(
                    requestGraph.GetRouteDescriptor(),
                    requestGraph.GetHttpRequestMessage());
        }
    }
}
