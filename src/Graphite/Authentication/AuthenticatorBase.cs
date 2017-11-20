namespace Graphite.Authentication
{
    public abstract class AuthenticatorBase : IAuthenticator
    {
        public string Realm { get; protected set; }
        public string UnauthorizedReasonPhrase { get; protected set; }
        public string Scheme { get; protected set; }

        public abstract bool Authenticate(string credentials);

        public virtual bool Applies()
        {
            return true;
        }
    }
}