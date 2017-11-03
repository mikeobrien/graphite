using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Graphite;
using Graphite.Actions;
using Graphite.Routing;
using Graphite.Setup;
using NUnit.Framework;
using Tests.Common;

public class NoNamespace { public void Post() {} }

namespace Tests.Unit.Routing
{
    public class Handler { public void Post() { } }

    [TestFixture]
    public class DefaultNamespaceUrlMappingConventionTests
    {
        private ConfigurationDsl.NamespaceUrlMappingDsl _mappingDsl;
        private List<UrlSegment> _methodSegments;
        private Configuration _configuration;
        private DefaultNamespaceUrlMappingConvention _convention;

        [SetUp]
        public void Setup()
        {
            _methodSegments = new List<UrlSegment>();
            _configuration = new Configuration();
            _mappingDsl = new ConfigurationDsl.NamespaceUrlMappingDsl(_configuration);
            _convention = new DefaultNamespaceUrlMappingConvention(_configuration);
        }

        public UrlContext CreateUrlContext<T>(Expression<Action<T>> method)
        {
            return new UrlContext(ActionMethod.From(method), 
                null, _methodSegments, null, null, null, null);
        }

        [Test]
        public void Should_not_fail_if_type_doesent_have_a_namespace()
        {
            _convention.GetUrls(CreateUrlContext<NoNamespace>(x => x.Post()))
                .ShouldOnlyContain("");
        }

        [Test]
        public void Should_not_map_a_mapping_that_doesent_match_namespace()
        {
            _mappingDsl.Clear()
                .Add("NoMatch", "nomatch")
                .Add(@"Tests\.Unit\.Routing", "match");

            _convention.GetUrls(CreateUrlContext<Handler>(x => x.Post()))
                .ShouldOnlyContain("match");
        }

        [Test]
        public void Should_match_multiple_namespaces()
        {
            _mappingDsl.Clear()
                .Add("^.*$", "all")
                .Add(@"Tests\.Unit\.Routing", "namespace");

            _convention.GetUrls(CreateUrlContext<Handler>(x => x.Post()))
                .ShouldOnlyContain("all", "namespace");
        }

        [Test]
        public void Should_split_on_delimiters(
            [Values(".", @"\", @"/")] string delimiter)
        {
            _mappingDsl.Clear()
                .Add(@"^Tests.Unit.Routing$", $"fark{delimiter}farker");

            _convention.GetUrls(CreateUrlContext<Handler>(x => x.Post()))
                .ShouldOnlyContain("fark/farker");
        }

        [Test]
        public void Should_remove_empty_segments(
            [Values("..", @"\\", @"//")] string delimiter)
        {
            _mappingDsl.Clear()
                .Add(@"^Tests.Unit.Routing$", $"fark{delimiter}farker");

            _convention.GetUrls(CreateUrlContext<Handler>(x => x.Post()))
                .ShouldOnlyContain("fark/farker");
        }

        [Test]
        public void Should_union_method_segments()
        {
            _methodSegments.AddRange(new UrlSegment("fark"), new UrlSegment("farker"));
            _convention.GetUrls(CreateUrlContext<Handler>(x => x.Post()))
                .ShouldOnlyContain("Unit/Routing/fark/farker");
        }
    }
}
