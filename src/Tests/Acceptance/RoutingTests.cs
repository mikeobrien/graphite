using System.Net;
using NUnit.Framework;
using Should;
using TestHarness;
using TestHarness.Routing;
using Tests.Common;

namespace Tests.Acceptance
{
    [TestFixture]
    public class RoutingTests
    {
        private const string BaseUrl = "Routing/";

        [Test]
        public void Should_include_path_in_namespace()
        {
            var result = Http.GetJson<Handler.OutputModel>($"{BaseUrl}SomeNamespace/SomeMethod");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Value.ShouldEqual("fark");
        }

        [Test]
        public void Should_create_url_alias([Values("Fark/Alias", BaseUrl + "NonAlias")] string url)
        {
            var result = Http.GetJson<RoutingTestHandler.UrlAliasModel>(url);

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Value.ShouldEqual("fark");
        }

        [TestCase("fark", "string:fark")]
        [TestCase("27ab17d7-1b82-4eae-a318-5db7b3c3e602", "guid:27ab17d7-1b82-4eae-a318-5db7b3c3e602")]
        public void Should_apply_route_constraints(string segment, string expected)
        {
            var result = Http.GetString($"{BaseUrl}WithAmbiguousTypeUrl/" + segment);

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.ShouldEqual(expected);
        }

        [TestCase("Segment", "no id")]
        [TestCase("27ab17d7-1b82-4eae-a318-5db7b3c3e602", "27ab17d7-1b82-4eae-a318-5db7b3c3e602")]
        public void Should_order_routes_properly(string segment, string expected)
        {
            var result = Http.GetString($"{BaseUrl}WithAmbiguousUrl/" + segment);

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.ShouldEqual(expected);
        }
    }
}
