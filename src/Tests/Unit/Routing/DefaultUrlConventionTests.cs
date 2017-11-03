using System;
using System.Collections.Generic;
using Graphite;
using Graphite.Actions;
using Graphite.Routing;
using NUnit.Framework;
using Tests.Common;
using Tests.Common.Fakes;

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

        private List<INamespaceUrlMappingConvention> _mappingConventions;
        private Configuration _configuration;

        [SetUp]
        public void Setup()
        {
            _configuration = new Configuration();
            _mappingConventions = new List<INamespaceUrlMappingConvention>
            {
                new DefaultNamespaceUrlMappingConvention(_configuration)
            };
        }

        [Test]
        public void Should_return_overriden_urls()
        {
            var actionMethod = ActionMethod.From<Handler>(x => x.Override());
            var urls = new DefaultUrlConvention(_mappingConventions, _configuration, null)
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
            var urls = new DefaultUrlConvention(_mappingConventions, _configuration, null)
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
            var urls = new DefaultUrlConvention(_mappingConventions, _configuration, null)
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
            var urls = new DefaultUrlConvention(_mappingConventions, _configuration, null)
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
            var urls = new DefaultUrlConvention(_mappingConventions, _configuration, null)
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
        public void Should_only_include_url_conventions_that_apply()
        {
            _mappingConventions.Clear();

            var applies1 = new TestUrlConvention(true, "applies1");
            var applies2 = new TestUrlConvention(true, "applies2");
            var appliesNot = new TestUrlConvention(false, "appliesnot");
            var appliesNotConfig = new TestUrlConvention(true, "appliesnot");

            _mappingConventions.Add(appliesNot);
            _mappingConventions.Add(appliesNotConfig);
            _mappingConventions.Add(applies2);
            _mappingConventions.Add(applies1);
            _configuration.NamespaceUrlMappingConventions.Configure(x => x
                .Clear()
                .Append(appliesNot)
                .Append(appliesNotConfig, c => false)
                .Append(applies2)
                .Append(applies1));

            var actionMethod = ActionMethod.From<Handler>(x => x.NoOverride());
            var urls = new DefaultUrlConvention(_mappingConventions, _configuration, null)
                .GetUrls(new UrlContext(actionMethod, null, 
                    new List<UrlSegment>(), null, null, null, null));

            urls.ShouldOnlyContain("applies2", "applies1");
        }

        public class TestUrlConvention : INamespaceUrlMappingConvention
        {
            private readonly bool _applies;
            private readonly string _url;

            public TestUrlConvention(bool applies, string url)
            {
                _applies = applies;
                _url = url;
            }

            public bool AppliesTo(UrlContext context) => _applies;
            public string[] GetUrls(UrlContext context) => new [] { _url };
        }
    }
}
