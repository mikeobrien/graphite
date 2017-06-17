using System;
using System.Net;
using Graphite.Authentication;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Acceptance
{
    [TestFixture]
    public class AuthenticationTests
    {
        [Test]
        public void Should_return_unauthorized_when_no_credentials_are_passed()
        {
            var result = Http.Post("Secure");

            result.Status.ShouldEqual(HttpStatusCode.Unauthorized);
        }

        [Test]
        public void Should_return_unauthorized_when_bad_bearer_token_is_passed()
        {
            var result = Http.Post("Secure", requestHeaders: x =>
                x.SetBearerTokenAuthorizationHeader("wrong"));

            result.Status.ShouldEqual(HttpStatusCode.Unauthorized);
        }

        [Test]
        public void Should_return_unauthorized_when_bad_credentials_are_passed()
        {
            var result = Http.Post("BasicSecure", requestHeaders: x =>
                x.SetBasicAuthorizationHeader("fark", "wrong"));

            result.Status.ShouldEqual(HttpStatusCode.Unauthorized);
        }

        [Test]
        public void Should_succeed_when_basic_credentials_are_passed()
        {
            var result = Http.Post("BasicSecure", requestHeaders: x => 
                x.SetBasicAuthorizationHeader("fark", "farker"));

            result.Status.ShouldEqual(HttpStatusCode.NoContent);
        }

        [Test]
        public void Should_succeed_when_bearer_token_is_passed(
            [Values("Secure", "BasicSecure")] string url)
        {
            var result = Http.Post(url, requestHeaders: x =>
                x.SetBearerTokenAuthorizationHeader("fark"));

            result.Status.ShouldEqual(HttpStatusCode.NoContent);
        }

        [Test]
        public void Should_return_unauthorized_when_authenticator_does_not_apply_to_action()
        {
            var result = Http.Post("Secure", requestHeaders: x =>
                x.SetBasicAuthorizationHeader("fark", "farker"));

            result.Status.ShouldEqual(HttpStatusCode.Unauthorized);
        }
    }
}
