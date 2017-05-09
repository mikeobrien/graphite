using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Graphite.Actions;

namespace Graphite.Behaviors
{
    public interface IBehaviorChainInvoker
    {
        Task<HttpResponseMessage> Invoke(ActionDescriptor actionDescriptor, 
            HttpRequestMessage requestMessage, CancellationToken cancellationToken);
    }
}