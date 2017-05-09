using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Graphite.Actions;
using Graphite.DependencyInjection;
using Graphite.Extensibility;
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
                container.Register(UrlParameters.CreateFrom(httpRequestContext));
                container.Register(QuerystringParameters.CreateFrom(requestMessage));

                IBehavior behaviorChain;
                try
                {
                    behaviorChain = actionDescriptor.Behaviors.AsEnumerable().Reverse()
                        .Aggregate(_configuration.DefaultBehavior.GetInstance(container),
                            (chain, type) =>
                            {
                                var behavior = container.GetInstance<IBehavior>(
                                    type.Type, Dependency.For(chain));
                                return behavior.ShouldRun() ? behavior : chain;
                            });
                }
                catch (Exception exception)
                {
                    throw new GraphiteRuntimeInitializationException(exception, 
                        requestMessage, actionDescriptor.Action, container);
                }

                return await behaviorChain.Invoke();
            }
        }
    }
}
