using System.Threading.Tasks;
using Graphite.Readers;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Readers
{
    [TestFixture]
    public class StringReaderTests
    {
        public class Handler
        {
            public void StringRequest(string request) { }
            public void NonStringRequest(object request) { }
        }

        [Test]
        public void Should_apply_if_the_request_type_is_string()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.StringRequest(null))
                    .WithRequestParameter("request");

            new StringReader().AppliesTo(requestGraph
                .GetRequestReaderContext()).ShouldBeTrue();
        }

        [Test]
        public void Should_not_apply_if_the_request_type_is_not_a_string()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.NonStringRequest(null))
                    .WithRequestParameter("request");

            new StringReader().AppliesTo(requestGraph
                .GetRequestReaderContext()).ShouldBeFalse();
        }

        [Test]
        public void Should_not_apply_if_no_request()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.StringRequest(null));

            new StringReader().AppliesTo(requestGraph
                .GetRequestReaderContext()).ShouldBeFalse();
        }

        [Test]
        public async Task Should_read_string()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.StringRequest(null))
                    .WithRequestParameter("request")
                    .WithRequestData("fark");

            var result = await new StringReader().Read(
                requestGraph.GetRequestReaderContext());

            result.ShouldEqual("fark");
        }
    }
}
