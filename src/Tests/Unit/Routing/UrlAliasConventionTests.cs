using Graphite;
using Graphite.Actions;
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
            var actionMethod = ActionMethod.From<AliasHandler>(x => x.Get());
            var urls = new AliasUrlConvention(new Configuration())
                .GetUrls(new UrlContext(actionMethod,
                    null, null, null, null, null, null));

            urls.ShouldOnlyContain("url1", "url2");
        }

        [Test]
        public void Should_return_conventional_url_aliases()
        {
            var actionMethod = ActionMethod.From<AliasHandler>(x => x.Post());
            var urls = new AliasUrlConvention(new Configuration
                {
                    UrlAliases =
                    {
                        (a, s) => $"{a.MethodDescriptor.Name}/url1/{s.ToString()}",
                        (a, s) => $"{a.MethodDescriptor.Name}/url2/{s.ToString()}"
                    }
                })
                .GetUrls(new UrlContext(actionMethod,
                    null, Url.Create("some", "url"), 
                    null, null, null, null));

            urls.ShouldOnlyContain("Post/url1/some/url", "Post/url2/some/url");
        }
    }
}
