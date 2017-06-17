namespace Graphite.Authentication
{
    public abstract class BearerTokenAuthenticatorBase : AuthenticatorBase
    {
        public const string BearerTokenScheme = "Bearer";

        protected BearerTokenAuthenticatorBase()
        {
            Scheme = BearerTokenScheme;
        }
    }
}