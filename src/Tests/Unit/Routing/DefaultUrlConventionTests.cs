using System.Collections.Generic;
using Graphite;
using Graphite.Actions;
using Graphite.Extensions;
using Graphite.Routing;
using NUnit.Framework;
using Should;
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
                    null, new List<Segment>
                    {
                        new Segment("some"),
                        new Segment("url")
                    },
                    null, null, null, null));

            urls.ShouldOnlyContain("url1", "url2");
        }

        [Test]
        public void Should_return_default_url_when_override_not_specified()
        {
            var actionMethod = ActionMethod.From<Handler>(x => x.NoOverride());
            var urls = new DefaultUrlConvention(new Configuration())
                .GetUrls(new UrlContext(actionMethod,
                    null, new List<Segment>
                    {
                        new Segment("some"),
                        new Segment("url")
                    }, 
                    null, null, null, null));

            urls.ShouldOnlyContain("Tests/Unit/Routing/some/url");
        }

        [TestCase(null, "Tests/Unit/Routing/some/url")]
        [TestCase("", "Tests/Unit/Routing/some/url")]
        [TestCase("/", "Tests/Unit/Routing/some/url")]
        [TestCase("///", "Tests/Unit/Routing/some/url")]
        [TestCase("/fark/", "fark/Tests/Unit/Routing/some/url")]
        [TestCase("fark/farker", "fark/farker/Tests/Unit/Routing/some/url")]
        public void Should_add_url_prefix(string prefix, string expected)
        {
            var actionMethod = ActionMethod.From<Handler>(x => x.NoOverride());
            var urls = new DefaultUrlConvention(new Configuration { UrlPrefix = prefix })
                .GetUrls(new UrlContext(actionMethod,
                    null, new List<Segment>
                    {
                        new Segment("some"),
                        new Segment("url")
                    },
                    null, null, null, null));

            urls.ShouldOnlyContain(expected);
        }

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
            var urls = new DefaultUrlConvention(new Configuration())
                .GetUrls(new UrlContext(actionMethod,
                    null, new List<Segment>
                    {
                        new Segment("some"),
                        new Segment("url")
                    }, null, null, null, null));

            urls.ShouldOnlyContain("Tests/Unit/Routing/some/url", "url1", "url2");
        }

        [Test]
        public void Should_return_conventional_url_aliases()
        {
            var actionMethod = ActionMethod.From<AliasHandler>(x => x.Post());
            var urls = new DefaultUrlConvention(new Configuration
                {
                    UrlAliases =
                    {
                        x => $"{x.ActionMethod.MethodDescriptor.Name}/url1/{x.MethodSegments.ToUrl()}",
                        x => $"{x.ActionMethod.MethodDescriptor.Name}/url2/{x.MethodSegments.ToUrl()}"
                    }
                })
                .GetUrls(new UrlContext(actionMethod,
                    null, new List<Segment>
                    {
                        new Segment("some"),
                        new Segment("url")
                    },
                    null, null, null, null));

            urls.ShouldOnlyContain("Tests/Unit/Routing/some/url", 
                "Post/url1/some/url", "Post/url2/some/url");
        }

        [Test]
        public void Should_match_handler_namespace_regex()
        {
            "fark".MatchGroupValue(DefaultUrlConvention.DefaultHandlerNamespaceConvention,
                    DefaultUrlConvention.HandlerNamespaceGroupName)
                .ShouldEqual("fark");
        }

        public class ParseNamespace
        {
            public void Post() { }
        }

        [Test]
        public void Should_parse_default_handler_namespace()
        {
            DefaultUrlConvention.DefaultHandlerNamespaceParser(new Configuration(), 
                    ActionMethod.From<ParseNamespace>(x => x.Post()))
                .ShouldEqual("Tests.Unit.Routing");
        }
    }
}
