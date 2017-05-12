using System;
using System.Threading.Tasks;
using Bender.Collections;
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
                    .AddParameter("param");

            if (isXml)
            {
                requestGraph.WithContentType(MimeTypes.ApplicationXml);
            }

            new XmlReader().AppliesTo(requestGraph.GetRequestReaderContext())
                .ShouldEqual(isXml);
        }

        [Test]
        public void Should_only_apply_if_the_action_has_a_request_parameter(
            [Values(true, false)] bool hasRequest)
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Post(null, null))
                    .WithRequestData("{}")
                    .WithContentType(MimeTypes.ApplicationXml)
                    .AddParameter("param");

            if (hasRequest)
            {
                requestGraph.WithRequestParameter("request");
            }

            new XmlReader().AppliesTo(requestGraph.GetRequestReaderContext())
                .ShouldEqual(hasRequest);
        }

        [Test]
        public async Task Should_read_xml()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Post(null, null))
                    .WithRequestData("<InputModel><Value>fark</Value></InputModel>")
                    .WithRequestParameter("request")
                    .WithContentType(MimeTypes.ApplicationXml)
                    .AddParameter("param");

            var result = await new XmlReader().Read(requestGraph.GetRequestBinderContext());

            result.ShouldNotBeNull();
            result.ShouldBeType<InputModel>();
            result.CastTo<InputModel>().Value.ShouldEqual("fark");
        }
    }
}
