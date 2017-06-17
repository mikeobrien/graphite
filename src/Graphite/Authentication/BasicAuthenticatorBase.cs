using Graphite.Extensions;

namespace Graphite.Authentication
{
    public abstract class BasicAuthenticatorBase : AuthenticatorBase
    {
        public const string BasicScheme = "Basic";

        protected BasicAuthenticatorBase()
        {
            Scheme = BasicScheme;
        }

        public abstract bool Authenticate(string username, string password);

        public sealed override bool Authenticate(string credentials)
        {
            if (credentials.IsNullOrEmpty()) return false;
            var parts = credentials.Split(new[] { ':' }, 2);
            return parts.Length == 2 && Authenticate(parts[0], parts[1]);
        }
    }
}