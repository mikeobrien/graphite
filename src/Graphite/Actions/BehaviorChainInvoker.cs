using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using Graphite.DependencyInjection;
using Graphite.Http;
using Graphite.Reflection;

namespace Graphite.Actions
{
    public class BehaviorChainInvoker : IBehaviorChainInvoker
    {
        private readonly IContainer _container;

        public BehaviorChainInvoker(IContainer container)
        {
            _container = container;
        }

        public virtual Task<HttpResponseMessage> Invoke(ActionDescriptor actionDescriptor, 
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using (var container = _container.CreateScopedContainer())
            {
                var httpRequestContext = request.GetRequestContext();
                var urlParameters = UrlParameters.CreateFrom(httpRequestContext);
                var querystringParameters = QuerystringParameters.CreateFrom(request);
                var requestContext = new RequestContext(actionDescriptor.Action,
                    actionDescriptor.Route, actionDescriptor.Behaviors, urlParameters,
                    querystringParameters, request, request.GetConfiguration(), 
                    cancellationToken);

                InitContainer(container, actionDescriptor, requestContext, request, 
                    httpRequestContext, cancellationToken);

                IBehavior behaviorChain;
                try
                {
                    behaviorChain = actionDescriptor.Behaviors.AsEnumerable().Reverse()
                        .Aggregate<TypeDescriptor, IBehavior>(
                            container.GetInstance<IInvokerBehavior>(),
                            (chain, type) =>
                            {
                                var behavior = container.GetInstance<IBehavior>(
                                    type.Type, Dependency.For(chain));
                                return behavior.ShouldRun() ? behavior : chain;
                            });
                }
                catch (Exception exception)
                {
                    throw new GraphiteRuntimeInitializationException(exception, requestContext);
                }
                return behaviorChain.Invoke();
            }
        }

        public virtual void InitContainer(IContainer container, ActionDescriptor actionDescriptor, 
            RequestContext requestContext, HttpRequestMessage request, HttpRequestContext httpRequestContext, 
            CancellationToken cancellationToken)
        {
            container.Register(container);
            container.IncludeRegistry(actionDescriptor.Registry);
            container.Register(request);
            container.Register(requestContext);
            container.Register(requestContext.Action);
            container.Register(requestContext.Route);
            container.Register(httpRequestContext);
            container.Register(new RequestCancellation(cancellationToken));
            container.Register(requestContext.UrlParameters);
            container.Register(requestContext.QuerystringParameters);           
        }
    }
}
