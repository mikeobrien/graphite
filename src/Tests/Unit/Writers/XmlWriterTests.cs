﻿using System.Threading.Tasks;
using System.Xml;
using Graphite;
using Graphite.Http;
using NUnit.Framework;
using Should;
using Tests.Common;
using XmlWriter = Graphite.Writers.XmlWriter;

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
                .CreateFor<Handler>(h => h.Post())
                    .WithAccept(MimeTypes.ApplicationXml); ;
            var context = requestGraph.GetResponseWriterContext(new OutputModel
            {
                Value1 = "value1",
                Value2 = "value2"
            });

            var response = await CreateWriter(requestGraph)
                .Write(context);

            var content = await response.Content.ReadAsStringAsync();

            content.ShouldEqual(
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<OutputModel " + 
                    "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
                    "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" + 
                "<Value1>value1</Value1>" +
                "<Value2>value2</Value2>" +
                "</OutputModel>");
        }

        private XmlWriter CreateWriter(RequestGraph requestGraph)
        {
            return new XmlWriter(
                requestGraph.GetHttpRequestMessage(), 
                requestGraph.GetHttpResponseMessage(),
                new Configuration(), new XmlWriterSettings());
        }
    }
}
