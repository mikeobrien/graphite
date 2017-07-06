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
        public void Should_include_path_in_namespace(
            [Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).GetJson<Handler.OutputModel>($"{BaseUrl}SomeNamespace/SomeMethod");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Value.ShouldEqual("fark");
        }

        [Test]
        public void Should_create_url_alias(
            [Values("Fark/Alias", BaseUrl + "NonAlias")] string url,
            [Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).GetJson<RoutingTestHandler.UrlAliasModel>(url);

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Value.ShouldEqual("fark");
        }

        [TestCase("fark", "string:fark", Host.IISExpress)]
        [TestCase("27ab17d7-1b82-4eae-a318-5db7b3c3e602", 
            "guid:27ab17d7-1b82-4eae-a318-5db7b3c3e602", Host.IISExpress)]

        [TestCase("fark", "string:fark", Host.Owin)]
        [TestCase("27ab17d7-1b82-4eae-a318-5db7b3c3e602",
            "guid:27ab17d7-1b82-4eae-a318-5db7b3c3e602", Host.Owin)]
        public void Should_apply_route_constraints(string segment, string expected, Host host)
        {
            var result = Http.ForHost(host).GetString($"{BaseUrl}WithAmbiguousTypeUrl/" + segment);

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.ShouldEqual(expected);
        }

        [TestCase("Segment", "no id", Host.IISExpress)]
        [TestCase("27ab17d7-1b82-4eae-a318-5db7b3c3e602", 
            "27ab17d7-1b82-4eae-a318-5db7b3c3e602", Host.IISExpress)]

        [TestCase("Segment", "no id", Host.Owin)]
        [TestCase("27ab17d7-1b82-4eae-a318-5db7b3c3e602",
            "27ab17d7-1b82-4eae-a318-5db7b3c3e602", Host.Owin)]
        public void Should_order_routes_properly(string segment, string expected, Host host)
        {
            var result = Http.ForHost(host).GetString($"{BaseUrl}WithAmbiguousUrl/" + segment);

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.ShouldEqual(expected);
        }

        [Test]
        public void Should_decorate_route([Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).Post($"{BaseUrl}WithRouteDecorator");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.ShouldEqual("fark");
        }
    }
}
