using System.Threading.Tasks;
using Graphite.Writers;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Writers
{
    [TestFixture]
    public class XmlWriterTests
    {
        public class InputModel { }

        public class OutputModel
        {
            public string Value1 { get; set; }
            public string Value2 { get; set; }
        }

        public class Handler
        {
            public OutputModel Post()
            {
                return null;
            }
        }

        [TestCase("text/html", false)]
        [TestCase("application/xml", true)]
        [TestCase("text/html,application/xml", true)]
        [TestCase("text/html,application/*", true)]
        [TestCase("text/html,*/*", true)]
        public void Should_only_apply_to_mime_types_that_are_accepted(
            string acceptType, bool applies)
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Post())
                    .WithAccept(acceptType);
            var context = requestGraph.GetResponseWriterContext(null);

            CreateWriter(requestGraph).AppliesTo(context).ShouldEqual(applies);
        }

        [Test]
        public async Task Should_write_xml()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Post());
            var context = requestGraph.GetResponseWriterContext(new OutputModel
            {
                Value1 = "value1",
                Value2 = "value2"
            });

            var response = await CreateWriter(requestGraph)
                .Write(context);

            var content = await response.Content.ReadAsStringAsync();

            content.ShouldEqual(
                "<?xml version=\"1.0\"?>\r\n" +
                "<OutputModel " + 
                    "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
                    "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">\r\n" + 
                "  <Value1>value1</Value1>\r\n" +
                "  <Value2>value2</Value2>\r\n" +
                "</OutputModel>");
        }

        private XmlWriter CreateWriter(RequestGraph requestGraph)
        {
            return new XmlWriter(requestGraph.GetRouteDescriptor(),
                requestGraph.GetHttpRequestMessage(), 
                requestGraph.GetHttpResponseMessage());
        }
    }
}
