using System.Net;
using System.Threading.Tasks;
using Graphite;
using Graphite.Writers;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Writers
{
    [TestFixture]
    public class RedirectWriterTests
    {
        public class RedirectModel : IRedirectable
        {
            private readonly HttpStatusCode? _status;
            private readonly string _url;

            public RedirectModel() { }

            public RedirectModel(HttpStatusCode? status, string url)
            {
                _status = status;
                _url = url;
            }

            HttpStatusCode? IRedirectable.RedirectStatus => _status;
            string IRedirectable.RedirectUrl => _url;

            public string Value { get; set; }
        }

        public class NotRedirectable { }

        public class Handler
        {
            public NotRedirectable NotRedirectable()
            {
                return null;
            }
            public Redirect Redirect()
            {
                return null;
            }

            public RedirectModel RedirectModel()
            {
                return null;
            }
        }

        [TestCase(HttpStatusCode.Found, null, true)]
        [TestCase(HttpStatusCode.Found, "", true)]
        [TestCase(null, "http://fark", false)]
        [TestCase(HttpStatusCode.Found, "http://fark", true)]
        public void Should_only_apply_to_actions_with_a_redirect(
            HttpStatusCode? status, string url, bool applies)
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.Redirect());
            CreateRedirectWriter(requestGraph).AppliesTo(requestGraph
                .GetResponseWriterContext(new Redirect(url, status))).ShouldEqual(applies);
        }

        [TestCase(HttpStatusCode.Found, null, true)]
        [TestCase(HttpStatusCode.Found, "", true)]
        [TestCase(null, "http://fark", false)]
        [TestCase(HttpStatusCode.Found, "http://fark", true)]
        public void Should_only_apply_to_action_models_with_a_redirect(
            HttpStatusCode? status, string url, bool applies)
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.RedirectModel());
            CreateRedirectWriter(requestGraph).AppliesTo(requestGraph
                .GetResponseWriterContext(new RedirectModel(status, url))).ShouldEqual(applies);
        }

        [Test]
        public void Should_not_apply_to_models_that_are_not_redirectable()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.NotRedirectable());
            CreateRedirectWriter(requestGraph).AppliesTo(requestGraph
                .GetResponseWriterContext(new NotRedirectable())).ShouldBeFalse();
        }

        [TestCase(HttpStatusCode.Found, "fark", "fark")]
        [TestCase(HttpStatusCode.Found, "/fark", "/fark")]
        [TestCase(HttpStatusCode.Found, "http://fark", "http://fark/")]
        [TestCase(HttpStatusCode.NotFound, null, null)]
        public async Task Should_redirect(HttpStatusCode? status, string url, string expected)
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.Redirect());

            var result = await CreateRedirectWriter(requestGraph).Write(requestGraph
                .GetResponseWriterContext(new Redirect(url, status)));

            result.StatusCode.ShouldEqual(status ?? HttpStatusCode.NoContent);
            (result.Headers.Location?.ToString()).ShouldEqual(expected);
        }

        [Test]
        public async Task Should_redirect_from_model()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.Redirect());

            var result = await CreateRedirectWriter(requestGraph).Write(requestGraph
                .GetResponseWriterContext(new RedirectModel(HttpStatusCode.Found, "http://fark")));

            result.StatusCode.ShouldEqual(HttpStatusCode.Found);
            result.Headers.Location.ToString().ShouldEqual("http://fark/");
        }

        private RedirectWriter CreateRedirectWriter(RequestGraph requestGraph)
        {
            return new RedirectWriter(
                requestGraph.GetRouteDescriptor(),
                requestGraph.GetHttpResponseMessage());
        }
    }
}
