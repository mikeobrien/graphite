using System;
using System.Linq;
using System.Net;
using System.Web.Cors;
using Graphite.Http;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Acceptance
{
    [TestFixture]
    public class CorsTests
    {
        private const string BaseUrl = "Cors/";

        [Test]
        public void Should_not_apply_cors_policy_to_non_cors_action(
            [Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).GetString($"{BaseUrl}NotCors");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.ShouldEqual("fark");
            result.Headers.Any(x => !x.Key.StartsWith("Access-Control-")).ShouldBeTrue();
        }

        [Test]
        public void Should_not_apply_cors_preflight_policy_to_non_cors_action(
            [Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).Options($"{BaseUrl}NotCors");

            result.Status.ShouldEqual(HttpStatusCode.NotFound);
        }

        [TestCase("1", "http://fark.com", true, Host.IISExpress)]
        [TestCase("1", "http://farker.com", false, Host.IISExpress)]
        [TestCase("2", "http://farker.com", true, Host.IISExpress)]
        [TestCase("2", "http://fark.com", false, Host.IISExpress)]

        [TestCase("1", "http://fark.com", true, Host.Owin)]
        [TestCase("1", "http://farker.com", false, Host.Owin)]
        [TestCase("2", "http://farker.com", true, Host.Owin)]
        [TestCase("2", "http://fark.com", false, Host.Owin)]
        public void Should_apply_policies_to_correct_actions(
            string policy, string url, bool valid, Host host)
        {
            var result = Http.ForHost(host).GetString($"{BaseUrl}CorsPolicy{policy}", 
                requestHeaders: x => x.Add(CorsConstants.Origin, url));

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.ShouldEqual("fark");

            if (valid)
            {
                result.Headers.Count(x => x.Key.StartsWith("Access-Control-")).ShouldEqual(1);
                result.Headers.GetValues(CorsConstants.AccessControlAllowOrigin)
                    .ShouldOnlyContain(url);
            }
            else
            {
                result.Headers.Any(x => !x.Key.StartsWith("Access-Control-")).ShouldBeTrue();
            }
        }

        [TestCase("1", "http://fark.com", true, Host.IISExpress)]
        [TestCase("1", "http://farker.com", false, Host.IISExpress)]
        [TestCase("2", "http://farker.com", true, Host.IISExpress)]
        [TestCase("2", "http://fark.com", false, Host.IISExpress)]

        [TestCase("1", "http://fark.com", true, Host.Owin)]
        [TestCase("1", "http://farker.com", false, Host.Owin)]
        [TestCase("2", "http://farker.com", true, Host.Owin)]
        [TestCase("2", "http://fark.com", false, Host.Owin)]
        public void Should_apply_policies_to_correct_actions_for_preflight(
            string policy, string url, bool valid, Host host)
        {
            var result = Http.ForHost(host).Options($"{BaseUrl}CorsPolicy{policy}", requestHeaders:
                x => {
                    x.Add(CorsConstants.Origin, url);
                    x.Add(CorsConstants.AccessControlRequestMethod, HttpMethod.Get.Method);
                });

            if (valid)
            {
                result.Status.ShouldEqual(HttpStatusCode.OK);
                result.Headers.Count(x => x.Key.StartsWith("Access-Control-")).ShouldEqual(1);
                result.Headers.GetValues(CorsConstants.AccessControlAllowOrigin)
                    .ShouldOnlyContain(url);
            }
            else
            {
                result.Status.ShouldEqual(HttpStatusCode.BadRequest);
                result.Headers.Any(x => !x.Key.StartsWith("Access-Control-")).ShouldBeTrue();
                result.Error.ShouldEqual($"The origin '{url}' is not allowed.");
            }
        }
    }
}
