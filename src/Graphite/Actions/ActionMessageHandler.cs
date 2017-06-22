using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Graphite.Behaviors;
using Graphite.Monitoring;

namespace Graphite.Actions
{
    public class ActionMessageHandler : HttpMessageHandler
    {
        private readonly ConfigurationContext _configurationContext;
        private readonly List<IInterceptor> _interceptors;
        private readonly ActionDescriptor _actionDescriptor;
        private readonly IUnhandledExceptionHandler _exceptionHandler;
        private readonly IBehaviorChainInvoker _behaviorChainInvoker;
        private readonly Metrics _metrics;
        private readonly ActionMetrics _actionMetrics;

        public ActionMessageHandler(ConfigurationContext configurationContext,
            List<IInterceptor> interceptors,
            ActionDescriptor actionDescriptor, 
            IUnhandledExceptionHandler exceptionHandler,
            IBehaviorChainInvoker behaviorChainInvoker, 
            Metrics metrics)
        {
            _actionDescriptor = actionDescriptor;
            _exceptionHandler = exceptionHandler;
            _behaviorChainInvoker = behaviorChainInvoker;
            _metrics = metrics;
            _configurationContext = configurationContext;
            _interceptors = interceptors;
            _actionMetrics = metrics.AddAction(_actionDescriptor);
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                if (_interceptors.Any())
                {
                    var inteceptorContext = new InterceptorContext(
                        _actionDescriptor, requestMessage, cancellationToken);
                    var interceptor = _interceptors.ThatAppliesTo(
                        inteceptorContext, _configurationContext);
                    if (interceptor != null) return await interceptor.Intercept(inteceptorContext);
                }
                return await _behaviorChainInvoker.Invoke(
                    _actionDescriptor, requestMessage, cancellationToken);
            }
            catch (Exception exception)
            {
                return _exceptionHandler.HandleException(exception,
                    _actionDescriptor, requestMessage);
            }
            finally
            {
                stopwatch.Stop();
                if (_configurationContext.Configuration.Metrics)
                {
                    _metrics.IncrementRequests();
                    _actionMetrics.AddRequestTime(stopwatch.Elapsed);
                }
            }
        }
    }
}
