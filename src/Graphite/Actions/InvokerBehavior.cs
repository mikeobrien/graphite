using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Behaviors;
using Graphite.DependencyInjection;

namespace Graphite.Actions
{
    public class InvokerBehavior : IBehavior
    {
        private readonly IContainer _container;
        private readonly ActionMethod _actionMethod;
        private readonly IActionInvoker _actionInvoker;

        public InvokerBehavior(IContainer container, ActionMethod actionMethod, 
            IActionInvoker actionInvoker)
        {
            _container = container;
            _actionMethod = actionMethod;
            _actionInvoker = actionInvoker;
        }

        public virtual bool ShouldRun()
        {
            return true;
        }

        public virtual Task<HttpResponseMessage> Invoke()
        {
            var handler = _container.GetInstance(_actionMethod.HandlerTypeDescriptor.Type);
            return _actionInvoker.Invoke(handler);
        }
    }
}
