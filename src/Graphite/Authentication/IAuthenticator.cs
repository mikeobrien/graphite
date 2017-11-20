using Graphite.Extensibility;

namespace Graphite.Authentication
{
    public interface IAuthenticator : IConditional
    {
        string Scheme { get; }
        string Realm { get; }
        string UnauthorizedReasonPhrase { get; }
        bool Authenticate(string credentials);
    }
}