using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Graphite;
using Graphite.Actions;
using Graphite.Routing;
using Graphite.Setup;
using NUnit.Framework;
using Tests.Common;

public class NoNamespace { public void Post() { } }

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

        private ConfigurationDsl.NamespaceUrlMappingDsl _mappingDsl;
        private List<UrlSegment> _methodSegments;
        private Configuration _configuration;

        [SetUp]
        public void Setup()
        {
            _configuration = new Configuration();
            _methodSegments = new List<UrlSegment>();
            _mappingDsl = new ConfigurationDsl.NamespaceUrlMappingDsl(_configuration);
        }

        [Test]
        public void Should_return_overriden_urls()
        {
            var actionMethod = ActionMethod.From<Handler>(x => x.Override());
            var urls = new DefaultUrlConvention(_configuration, null)
                .GetUrls(new UrlContext(actionMethod,
                    null, new List<UrlSegment>
                    {
                        new UrlSegment("some"),
                        new UrlSegment("url")
                    },
                    null, null, null, null));

            urls.ShouldOnlyContain("url1", "url2");
        }

        [Test]
        public void Should_return_default_url_when_override_not_specified()
        {
            var actionMethod = ActionMethod.From<Handler>(x => x.NoOverride());
            var urls = new DefaultUrlConvention(_configuration, null)
                .GetUrls(new UrlContext(actionMethod,
                    null, new List<UrlSegment>
                    {
                        new UrlSegment("some"),
                        new UrlSegment("url")
                    },
                    null, null, null, null));

            urls.ShouldOnlyContain("Unit/Routing/some/url");
        }

        [TestCase(null, "Unit/Routing/some/url")]
        [TestCase("", "Unit/Routing/some/url")]
        [TestCase("/", "Unit/Routing/some/url")]
        [TestCase("///", "Unit/Routing/some/url")]
        [TestCase("/fark/", "fark/Unit/Routing/some/url")]
        [TestCase("fark/farker", "fark/farker/Unit/Routing/some/url")]
        public void Should_add_url_prefix(string prefix, string expected)
        {
            var actionMethod = ActionMethod.From<Handler>(x => x.NoOverride());
            _configuration.UrlPrefix = prefix;
            var urls = new DefaultUrlConvention(_configuration, null)
                .GetUrls(new UrlContext(actionMethod,
                    null, new List<UrlSegment>
                    {
                        new UrlSegment("some"),
                        new UrlSegment("url")
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
            var urls = new DefaultUrlConvention(_configuration, null)
                .GetUrls(new UrlContext(actionMethod,
                    null, new List<UrlSegment>
                    {
                        new UrlSegment("some"),
                        new UrlSegment("url")
                    }, null, null, null, null));

            urls.ShouldOnlyContain("Unit/Routing/some/url", "url1", "url2");
        }

        [Test]
        public void Should_return_conventional_url_aliases()
        {
            var actionMethod = ActionMethod.From<AliasHandler>(x => x.Post());
            _configuration.UrlAliases.Add(x => $"{x.ActionMethod.MethodDescriptor.Name}/url1/{x.MethodSegments.ToUrl()}");
            _configuration.UrlAliases.Add(x => $"{x.ActionMethod.MethodDescriptor.Name}/url2/{x.MethodSegments.ToUrl()}");
            var urls = new DefaultUrlConvention(_configuration, null)
                .GetUrls(new UrlContext(actionMethod,
                    null, new List<UrlSegment>
                    {
                        new UrlSegment("some"),
                        new UrlSegment("url")
                    },
                    null, null, null, null));

            urls.ShouldOnlyContain("Unit/Routing/some/url",
                "Post/url1/some/url", "Post/url2/some/url");
        }

        [Test]
        public void Should_not_fail_if_type_doesent_have_a_namespace()
        {
            new DefaultUrlConvention(_configuration, null)
                .GetUrls(CreateUrlContext<NoNamespace>(x => x.Post()))
                .ShouldOnlyContain("");
        }

        [Test]
        public void Should_not_map_a_mapping_that_doesent_match_namespace()
        {
            _mappingDsl.Clear()
                .Add("NoMatch", "nomatch")
                .Add(@"Tests\.Unit\.Routing", "match");

            new DefaultUrlConvention(_configuration, null)
                .GetUrls(CreateUrlContext<Handler>(x => x.NoOverride()))
                .ShouldOnlyContain("match");
        }

        [Test]
        public void Should_match_multiple_namespaces()
        {
            _mappingDsl.Clear()
                .Add("^.*$", "all")
                .Add(@"Tests\.Unit\.Routing", "namespace");

            new DefaultUrlConvention(_configuration, null)
                .GetUrls(CreateUrlContext<Handler>(x => x.NoOverride()))
                .ShouldOnlyContain("all", "namespace");
        }

        [Test]
        public void Should_split_on_delimiters(
            [Values(".", @"\", @"/")] string delimiter)
        {
            _mappingDsl.Clear()
                .Add(@"^Tests.Unit.Routing$", $"fark{delimiter}farker");

            new DefaultUrlConvention(_configuration, null)
                .GetUrls(CreateUrlContext<Handler>(x => x.NoOverride()))
                .ShouldOnlyContain("fark/farker");
        }

        [Test]
        public void Should_remove_empty_segments(
            [Values("..", @"\\", @"//")] string delimiter)
        {
            _mappingDsl.Clear()
                .Add(@"^Tests.Unit.Routing$", $"fark{delimiter}farker");

            new DefaultUrlConvention(_configuration, null)
                .GetUrls(CreateUrlContext<Handler>(x => x.NoOverride()))
                .ShouldOnlyContain("fark/farker");
        }

        [Test]
        public void Should_union_method_segments()
        {
            _methodSegments.AddRange(new UrlSegment("fark"), new UrlSegment("farker"));
            new DefaultUrlConvention(_configuration, null)
                .GetUrls(CreateUrlContext<Handler>(x => x.NoOverride()))
                .ShouldOnlyContain("Unit/Routing/fark/farker");
        }

        public UrlContext CreateUrlContext<T>(Expression<Action<T>> method)
        {
            return new UrlContext(ActionMethod.From(method),
                null, _methodSegments, null, null, null, null);
        }
    }
}
