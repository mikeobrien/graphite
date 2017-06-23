using System.Net.Http;
using System.Threading.Tasks;

namespace Graphite.Behaviors
{
    public interface IBehaviorChain
    {
        Task<HttpResponseMessage> InvokeNext();
    }
}