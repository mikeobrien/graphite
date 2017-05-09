using Graphite;
using Graphite.Actions;
using Graphite.Routing;
using NUnit.Framework;
using Tests.Common;

namespace Tests.Unit.Routing
{
    [TestFixture]
    public class DefaultUrlConventionTests
    {
        public class Handler
        {
            [Url("url1", "url2/")]
            public void Override() { }

            public void NoOverride() { }
        }

        [Test]
        public void Should_return_overriden_urls()
        {
            var actionMethod = ActionMethod.From<Handler>(x => x.Override());
            var urls = new DefaultUrlConvention(new Configuration())
                .GetUrls(new UrlContext(actionMethod,
                    null, Url.Create("some", "url"),
                    null, null, null, null));

            urls.ShouldOnlyContain("url1", "url2");
        }

        [Test]
        public void Should_return_default_url_when_override_not_specified()
        {
            var actionMethod = ActionMethod.From<Handler>(x => x.NoOverride());
            var urls = new DefaultUrlConvention(new Configuration())
                .GetUrls(new UrlContext(actionMethod,
                    null, Url.Create("some", "url"), 
                    null, null, null, null));

            urls.ShouldOnlyContain("some/url");
        }

        [TestCase(null, "some/url")]
        [TestCase("", "some/url")]
        [TestCase("/", "some/url")]
        [TestCase("///", "some/url")]
        [TestCase("/fark/", "fark/some/url")]
        [TestCase("fark/farker", "fark/farker/some/url")]
        public void Should_add_url_prefix(string prefix, string expected)
        {
            var actionMethod = ActionMethod.From<Handler>(x => x.NoOverride());
            var urls = new DefaultUrlConvention(new Configuration { UrlPrefix = prefix })
                .GetUrls(new UrlContext(actionMethod,
                    null, Url.Create("some", "url"),
                    null, null, null, null));

            urls.ShouldOnlyContain(expected);
        }
    }
}
