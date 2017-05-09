using System.Threading;

namespace Graphite.Http
{
    public class RequestCancellation
    {
        public RequestCancellation(CancellationToken token)
        {
            Token = token;
        }

        public CancellationToken Token { get; }
    }
}