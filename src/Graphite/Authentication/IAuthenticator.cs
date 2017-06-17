using Graphite.Extensibility;

namespace Graphite.Authentication
{
    public interface IAuthenticator : IConditional
    {
        string Scheme { get; }
        string Realm { get; }
        string UnauthorizedStatusMessage { get; }
        bool Authenticate(string credentials);
    }
}