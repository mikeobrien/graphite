using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Graphite.Actions;
using Graphite.DependencyInjection;
using Graphite.Http;

namespace Graphite.Behaviors
{
    public class BehaviorChainInvoker : IBehaviorChainInvoker
    {
        private readonly Configuration _configuration;
        private readonly IContainer _container;

        public BehaviorChainInvoker(Configuration configuration, IContainer container)
        {
            _configuration = configuration;
            _container = container;
        }

        public virtual async Task<HttpResponseMessage> Invoke(ActionDescriptor actionDescriptor, 
            HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            using (var container = _container.CreateScopedContainer())
            {
                Register(container, actionDescriptor, requestMessage, cancellationToken);

                return await container.GetInstance<IBehaviorChain>(
                    _configuration.BehaviorChain).InvokeNext();
            }
        }

        public virtual void Register(IContainer container, ActionDescriptor actionDescriptor,
            HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            var httpRequestContext = requestMessage.GetRequestContext();
            var responseMessage = requestMessage.CreateResponse();
            container.IncludeRegistry(actionDescriptor.Registry);

            container.Register(container);
            container.Register(requestMessage);
            container.Register(responseMessage);
            container.Register(responseMessage.Headers);
            container.Register(actionDescriptor);
            container.Register(actionDescriptor.Action);
            container.Register(actionDescriptor.Route);
            container.Register(httpRequestContext);
            container.Register(new RequestCancellation(cancellationToken));
            container.Register(requestMessage.UrlParameters());
            container.Register(requestMessage.Querystring());
        }
    }
}
