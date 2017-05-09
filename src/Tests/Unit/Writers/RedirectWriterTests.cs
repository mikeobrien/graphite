using System.Net;
using System.Threading.Tasks;
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
            private readonly RedirectType _type;
            private readonly string _url;

            public RedirectModel()
            {
                _type = RedirectType.None;
            }

            public RedirectModel(RedirectType type, string url)
            {
                _type = type;
                _url = url;
            }

            RedirectType IRedirectable.Ridirect => _type;
            string IRedirectable.RidirectUrl => _url;

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

        [TestCase(RedirectType.Found, null, false)]
        [TestCase(RedirectType.Found, "", false)]
        [TestCase(RedirectType.None, "http://fark", false)]
        [TestCase(RedirectType.Found, "http://fark", true)]
        public void Should_only_apply_to_actions_with_a_redirect(
            RedirectType type, string url, bool applies)
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.Redirect());
            CreateRedirectWriter(requestGraph).AppliesTo(requestGraph
                .GetResponseWriterContext(new Redirect { Type = type, Url = url })).ShouldEqual(applies);
        }

        [TestCase(RedirectType.Found, null, false)]
        [TestCase(RedirectType.Found, "", false)]
        [TestCase(RedirectType.None, "http://fark", false)]
        [TestCase(RedirectType.Found, "http://fark", true)]
        public void Should_only_apply_to_action_models_with_a_redirect(
            RedirectType type, string url, bool applies)
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.RedirectModel());
            CreateRedirectWriter(requestGraph).AppliesTo(requestGraph
                .GetResponseWriterContext(new RedirectModel(type, url))).ShouldEqual(applies);
        }

        [Test]
        public void Should_not_apply_to_models_that_are_not_redirectable()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.NotRedirectable());
            CreateRedirectWriter(requestGraph).AppliesTo(requestGraph
                .GetResponseWriterContext(new NotRedirectable())).ShouldBeFalse();
        }

        [TestCase("fark", "fark")]
        [TestCase("/fark", "/fark")]
        [TestCase("http://fark", "http://fark/")]
        public async Task Should_redirect(string url, string expected)
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.Redirect());

            var result = await CreateRedirectWriter(requestGraph).Write(requestGraph
                .GetResponseWriterContext(new Redirect
                {
                    Type = RedirectType.Found,
                    Url = url
                }));

            result.StatusCode.ShouldEqual(HttpStatusCode.Found);
            result.Headers.Location.ToString().ShouldEqual(expected);
        }

        [Test]
        public async Task Should_redirect_from_model()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.Redirect());

            var result = await CreateRedirectWriter(requestGraph).Write(requestGraph
                .GetResponseWriterContext(new RedirectModel(RedirectType.Found, "http://fark")));

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
