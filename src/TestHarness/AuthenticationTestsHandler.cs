using Graphite.Authentication;

namespace TestHarness
{
    public class AuthenticationTestsHandler
    {
        public void Post_Secure() { }
        public void Post_BasicSecure() { }
    }

    public class TestBasicAuthenticator : BasicAuthenticatorBase
    {
        public override bool Authenticate(string username, string password)
        {
            return username == "fark" && password == "farker";
        }
    }

    public class TestBearerTokenAuthenticator : BearerTokenAuthenticatorBase
    {
        public override bool Authenticate(string credentials)
        {
            return credentials == "fark";
        }
    }
}