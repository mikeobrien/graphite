using Graphite.Authentication;

namespace Tests.Unit.Authentication
{
    public class TestBasicAuthenticator : BasicAuthenticatorBase
    {
        private readonly string _username;
        private readonly string _password;

        public TestBasicAuthenticator(string username, string password)
        {
            _username = username;
            _password = password;
        }

        public string RealmOverride { set { Realm = value;  } }
        public string StatusMessageOverride { set { UnauthorizedStatusMessage = value; } }

        public override bool Authenticate(string username, string password)
        {
            return username == _username && password == _password;
        }
    }

    public class TestBearerTokenAuthenticator : BearerTokenAuthenticatorBase
    {
        private readonly string _token;

        public TestBearerTokenAuthenticator(string token)
        {
            _token = token;
        }
        
        public override bool Authenticate(string credentials)
        {
            return credentials == _token;
        }
    }
}