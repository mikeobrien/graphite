using System.Linq;
using System.Net.Http;
using Graphite.Authentication;
using Graphite.Extensions;
using NUnit.Framework;
using Should;

namespace Tests.Unit.Authentication
{
    [TestFixture]
    public class ExtensionTests
    {
        [Test]
        public void Should_set_basic_authorization_header()
        {
            var request = new HttpRequestMessage();

            request.SetBasicAuthorizationHeader("fark", "farker");

            request.Headers.Authorization.Scheme.ShouldEqual(
                BasicAuthenticatorBase.BasicScheme);
            request.Headers.Authorization.Parameter
                .FromBase64().ShouldEqual("fark:farker");
        }

        [Test]
        public void Should_set_bearer_token_authorization_header()
        {
            var request = new HttpRequestMessage();

            request.SetBearerTokenAuthorizationHeader("fark");

            request.Headers.Authorization.Scheme.ShouldEqual(
                BearerTokenAuthenticatorBase.BearerTokenScheme);
            request.Headers.Authorization.Parameter.ShouldEqual("fark");
        }

        [TestCase(null, null)]
        [TestCase("", null)]
        [TestCase("fark", "realm=\"fark\"")]
        public void Should_add_authenticate_header(string realm, string parameters)
        {
            var response = new HttpResponseMessage().AddAuthenticateHeader("type", realm);

            response.Headers.Count().ShouldEqual(1);
            var header = response.Headers.WwwAuthenticate.First();
            
            header.Scheme.ShouldEqual("type");
            header.Parameter.ShouldEqual(parameters);
        }
    }
}
