using Graphite.Routing;
using NUnit.Framework;
using Tests.Common;
using Graphite.Extensions;

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
            var actionMethod = Type<Handler>.Method(x => x.Override()).ToActionMethod<Handler>();
            var urls = new DefaultUrlConvention()
                .GetUrls(new UrlContext(null, null, actionMethod,
                    null, new[] { "some", "url" },
                    null, null, null, null, null));

            urls.ShouldOnlyContain("url1", "url2");
        }

        [Test]
        public void Should_return_default_url_when_override_not_specified()
        {
            var actionMethod = Type<Handler>.Expression(x => x.NoOverride()).ToActionMethod();
            var urls = new DefaultUrlConvention()
                .GetUrls(new UrlContext(null, null, actionMethod,
                    null, new []  { "some", "url" }, 
                    null, null, null, null, null));

            urls.ShouldOnlyContain("some/url");
        }
    }
}
