using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Graphite.Behaviors;
using Graphite.Monitoring;

namespace Graphite.Actions
{
    public class ActionMessageHandler : HttpMessageHandler
    {
        private readonly Configuration _configuration;
        private readonly ActionDescriptor _actionDescriptor;
        private readonly IUnhandledExceptionHandler _exceptionHandler;
        private readonly IBehaviorChainInvoker _behaviorChainInvoker;
        private readonly Metrics _metrics;
        private readonly ActionMetrics _actionMetrics;

        public ActionMessageHandler(Configuration configuration,
            ActionDescriptor actionDescriptor,
            IUnhandledExceptionHandler exceptionHandler,
            IBehaviorChainInvoker behaviorChainInvoker,
            Metrics metrics)
        {
            _configuration = configuration;
            _actionDescriptor = actionDescriptor;
            _exceptionHandler = exceptionHandler;
            _behaviorChainInvoker = behaviorChainInvoker;
            _metrics = metrics;
            _actionMetrics = metrics.AddAction(_actionDescriptor);
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
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
                if (_configuration.Metrics)
                {
                    _metrics.IncrementRequests();
                    _actionMetrics.AddRequestTime(stopwatch.Elapsed);
                }
            }
        }
    }
}
