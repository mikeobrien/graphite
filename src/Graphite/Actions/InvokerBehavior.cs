using System.Net.Http;
using System.Threading.Tasks;
using Graphite.DependencyInjection;

namespace Graphite.Actions
{
    public interface IInvokerBehavior : IBehavior { }

    public class InvokerBehavior : IInvokerBehavior
    {
        private readonly IContainer _container;
        private readonly RequestContext _requestContext;
        private readonly IActionInvoker _actionInvoker;

        public InvokerBehavior(IContainer container, RequestContext requestContext,
            IActionInvoker actionInvoker)
        {
            _container = container;
            _requestContext = requestContext;
            _actionInvoker = actionInvoker;
        }

        public virtual bool ShouldRun()
        {
            return true;
        }

        public virtual Task<HttpResponseMessage> Invoke()
        {
            var handler = _container.GetInstance(_requestContext.Action.HandlerTypeDescriptor.Type);
            return _actionInvoker.Invoke(handler);
        }
    }
}
