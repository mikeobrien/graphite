using Graphite;
using Graphite.Routing;
using NUnit.Framework;
using Tests.Common;
using Graphite.Extensions;

namespace Tests.Unit.Routing
{
    [TestFixture]
    public class UrlAliasConventionTests
    {
        public class AliasHandler
        {
            [UrlAlias("url1", "url2/")]
            public void Get() { }

            public void Post() { }
        }

        [Test]
        public void Should_return_attribute_url_aliases()
        {
            var actionMethod = Type<AliasHandler>.Method(x => x.Get()).ToActionMethod<AliasHandler>();
            var urls = new AliasUrlConvention(new Configuration())
                .GetUrls(new UrlContext(null, null, actionMethod,
                    null, null, null, null, null, null, null));

            urls.ShouldOnlyContain("url1", "url2");
        }

        [Test]
        public void Should_return_conventional_url_aliases()
        {
            var actionMethod = Type<AliasHandler>.Expression(x => x.Post()).ToActionMethod();
            var urls = new AliasUrlConvention(new Configuration
                {
                    UrlAliases =
                    {
                        (a, s) => $"{a.Method.Name}/url1/{s.Join("/")}",
                        (a, s) => $"{a.Method.Name}/url2/{s.Join("/")}"
                    }
                })
                .GetUrls(new UrlContext(null, null, actionMethod,
                    null, new []  { "some", "url" }, 
                    null, null, null, null, null));

            urls.ShouldOnlyContain("Post/url1/some/url", "Post/url2/some/url");
        }
    }
}
