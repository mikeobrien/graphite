using System;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace MyWebApp
{
    public class Marker { }
    namespace Api
    {
        public class Marker { }
        namespace Users
        {
            public class Marker { }
            public class Handler
            {
                public void Action() { }
            }
        }
    }
}

namespace Tests.Unit
{
    [TestFixture]
    public class ConfigurationDslTests
    {
        [TestCase(typeof(MyWebApp.Marker), "Api.Users")]
        [TestCase(typeof(MyWebApp.Api.Marker), "Users")]
        [TestCase(typeof(MyWebApp.Api.Users.Marker), "")]
        public void Should_exclude_type_namespace_from_url(
            Type marker, string expected)
        {
            var request = RequestGraph.CreateFor<MyWebApp
                .Api.Users.Handler>(x => x.Action());
            request.Configure(x => x.ExcludeTypeNamespaceFromUrl(marker));

            var @namespace = request.Configuration.HandlerNamespaceParser(
                request.Configuration, request.ActionMethod);

            @namespace.ShouldEqual(expected);
        }
    }
}
