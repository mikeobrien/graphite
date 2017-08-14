using System.Net;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Acceptance
{
    [TestFixture]
    public class ActionTests
    {
        private const string BaseUrl = "Action/";

        [Test]
        public void Should_modify_cookies([Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).Get($"{BaseUrl}UpdateCookies");

            result.Status.ShouldEqual(HttpStatusCode.NoContent);
            result.Cookies["fark"].ShouldEqual("farker");
        }

        [Test]
        public void Should_modify_headers([Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).Get($"{BaseUrl}UpdateHeaders");

            result.Status.ShouldEqual(HttpStatusCode.NoContent);
            result.Headers.GetValues("fark").ShouldContain("farker");
        }

        [Test]
        public void Should_write_http_response_message(
            [Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).Get($"{BaseUrl}WithResponseMessage");

            result.Status.ShouldEqual(HttpStatusCode.PaymentRequired);
        }

        [Test]
        public void Should_get_exception_details(
            [Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).Get($"{BaseUrl}Exception");

            result.Status.ShouldEqual(HttpStatusCode.InternalServerError);
            result.Error.ShouldContainAll("fark", "System.Exception");
        }

        [Test]
        public void Should_not_get_exception_details(
            [Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).Get($"{BaseUrl}Exception?_error=false");

            result.Status.ShouldEqual(HttpStatusCode.InternalServerError);
            result.Error.ShouldBeEmpty();
        }
    }
}
