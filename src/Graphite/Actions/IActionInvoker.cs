using System.Net.Http;
using System.Threading.Tasks;

namespace Graphite.Actions
{
    public interface IActionInvoker
    {
        Task<HttpResponseMessage> Invoke(object handler);
    }
}