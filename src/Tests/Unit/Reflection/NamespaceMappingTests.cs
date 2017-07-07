using Graphite.Reflection;
using NUnit.Framework;
using Should;

namespace Tests.Unit.Reflection
{
    [TestFixture]
    public class NamespaceMappingTests
    {

        [TestCase("Root", "")]
        [TestCase("Root.Child", "Child")]
        [TestCase("Root.Child1.Child2", "Child1.Child2")]
        public void Should_map_namespace_to_url(string @namespace, string expected)
        {
            var mapping = NamespaceMapping.DefaultMapping;
            mapping.Map(@namespace, '.')
                .ShouldEqual(expected);
        }

        [TestCase("MyWebApp", "Api.Users")]
        [TestCase("MyWebApp.Api", "Users")]
        [TestCase("MyWebApp.Api.Users", "")]
        public void Should_map_namespace_after(string @namespace, string expected)
        {
            var mapping = NamespaceMapping.MapAfterNamespace(@namespace);
            var url = mapping.Map("MyWebApp.Api.Users", '.');

            url.ShouldEqual(expected);
        }
    }
}
