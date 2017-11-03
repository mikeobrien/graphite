using Graphite.Routing;
using NUnit.Framework;
using Should;

namespace Tests.Unit.Routing
{
    [TestFixture]
    public class NamespaceUrlMappingTests
    {
        [TestCase("Root", "")]
        [TestCase("Root.Child", "Child")]
        [TestCase("Root.Child1.Child2", "Child1.Child2")]
        public void Should_map_namespace_to_url(string @namespace, string expected)
        {
            var mapping = NamespaceUrlMapping.DefaultMapping;
            mapping.Namespace.Replace(@namespace, mapping.Url)
                .ShouldEqual(expected);
        }

        [TestCase("MyWebApp", "Api.Users")]
        [TestCase("MyWebApp.Api", "Users")]
        [TestCase("MyWebApp.Api.Users", "")]
        public void Should_map_namespace_after(string @namespace, string expected)
        {
            var mapping = NamespaceUrlMapping.MapAfterNamespace(@namespace);
            var url = mapping.Namespace.Replace(
                "MyWebApp.Api.Users", mapping.Url);

            url.ShouldEqual(expected);
        }
    }
}
