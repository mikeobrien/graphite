using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using Graphite.Actions;
using Graphite.DependencyInjection;
using Graphite.Http;
using Graphite.Reflection;

namespace Graphite.Behaviors
{
    public class BehaviorChainInvoker : IBehaviorChainInvoker
    {
        private readonly IContainer _container;
        private readonly Configuration _configuration;

        public BehaviorChainInvoker(IContainer container, Configuration configuration)
        {
            _container = container;
            _configuration = configuration;
        }

        public virtual async Task<HttpResponseMessage> Invoke(ActionDescriptor actionDescriptor, 
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
                    httpRequestContext, cancellationToken);

                InitContainer(container, actionDescriptor, requestContext, request,
                    httpRequestContext, cancellationToken);

                IBehavior behaviorChain;
                try
                {
                    behaviorChain = actionDescriptor.Behaviors.AsEnumerable().Reverse()
                        .Aggregate(container.GetInstance<IBehavior>(_configuration.DefaultBehavior.Type),
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
                return await behaviorChain.Invoke();
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
