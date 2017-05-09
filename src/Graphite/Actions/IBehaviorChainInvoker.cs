using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Graphite.Actions
{
    public interface IBehaviorChainInvoker
    {
        Task<HttpResponseMessage> Invoke(ActionDescriptor actionDescriptor, 
            HttpRequestMessage request, CancellationToken cancellationToken);
    }
}