using System.Threading.Tasks;
using Graphite.Http;
using Graphite.Writers;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Writers
{
    [TestFixture]
    public class StringWriterTests
    {
        public class Handler
        {
            public string StringResponse()
            {
                return null;
            }

            public void NoResponse() { }

            public int NonStringResponse()
            {
                return 0;
            }
        }

        [Test]
        public void Should_only_apply_to_actions_returning_string()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.StringResponse());
            new StringWriter().AppliesTo(requestGraph
                .GetResponseWriterContext(null)).ShouldBeTrue();
        }

        [Test]
        public void Should_not_apply_to_actions_returning_non_string()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.NonStringResponse());
            new StringWriter().AppliesTo(requestGraph
                .GetResponseWriterContext(null)).ShouldBeFalse();
        }

        [Test]
        public void Should_not_apply_to_actions_with_no_return()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.NoResponse());
            new StringWriter().AppliesTo(requestGraph
                .GetResponseWriterContext(null)).ShouldBeFalse();
        }

        [TestCase(null, null)]
        [TestCase("", MimeTypes.TextPlain)]
        [TestCase("fark", MimeTypes.TextPlain)]
        [TestCase("<b>fark</b>", MimeTypes.TextHtml)]
        public async Task Should_write_string(string value, string contentType)
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.NoResponse());
            var result = await new StringWriter().Write(requestGraph
                .GetResponseWriterContext(value));

            (result.Content?.ReadAsStringAsync().Result).ShouldEqual(value);
            (result.Content?.Headers.ContentType.MediaType).ShouldEqual(contentType);
        }
    }
}
