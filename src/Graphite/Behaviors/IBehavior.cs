using System.Net.Http;
using System.Threading.Tasks;

namespace Graphite.Behaviors
{
    public interface IBehavior
    {
        bool ShouldRun();
        Task<HttpResponseMessage> Invoke();
    }
}
