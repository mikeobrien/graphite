using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Graphite.Extensibility;

namespace Graphite.Actions
{
    public class InterceptorContext
    {
        public InterceptorContext(ActionDescriptor actionDescriptor, 
            HttpRequestMessage requestMessage, 
            CancellationToken cancellationToken)
        {
            ActionDescriptor = actionDescriptor;
            RequestMessage = requestMessage;
            CancellationToken = cancellationToken;
        }

        public ActionDescriptor ActionDescriptor { get; }
        public HttpRequestMessage RequestMessage { get; }
        public CancellationToken CancellationToken { get; }
    }

    public interface IInterceptor : IConditional<InterceptorContext>
    {
        Task<HttpResponseMessage> Intercept(InterceptorContext context);
    }
}
