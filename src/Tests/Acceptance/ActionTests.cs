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
        public void Should_modify_cookies()
        {
            var result = Http.Get($"{BaseUrl}UpdateCookies");

            result.Status.ShouldEqual(HttpStatusCode.NoContent);
            result.Cookies["fark"].ShouldEqual("farker");
        }

        [Test]
        public void Should_modify_headers()
        {
            var result = Http.Get($"{BaseUrl}UpdateHeaders");

            result.Status.ShouldEqual(HttpStatusCode.NoContent);
            result.Headers.GetValues("fark").ShouldContain("farker");
        }

        [Test]
        public void Should_write_http_response_message()
        {
            var result = Http.Get($"{BaseUrl}WithResponseMessage");

            result.Status.ShouldEqual(HttpStatusCode.PaymentRequired);
        }
    }
}
