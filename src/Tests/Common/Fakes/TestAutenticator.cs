using Graphite.Authentication;

namespace Tests.Common.Fakes
{
    public class TestAutenticatorBase : IAuthenticator
    {
        public string Scheme { get; }
        public string Realm { get; }
        public string UnauthorizedStatusMessage { get; }

        public bool Applies()
        {
            throw new System.NotImplementedException();
        }
        public bool Authenticate(string credentials)
        {
            throw new System.NotImplementedException();
        }
    }

    public class TestAutenticator1 : TestAutenticatorBase { }
    public class TestAutenticator2 : TestAutenticatorBase { }
}
