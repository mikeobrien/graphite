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
        private readonly ActionDescriptor _actionDescriptor;
        private readonly IBehaviorChainInvoker _behaviorChainInvoker;
        private readonly Metrics _metrics;
        private readonly Configuration _configuration;
        private readonly ActionMetrics _actionMetrics;

        public ActionMessageHandler(ActionDescriptor actionDescriptor, 
            IBehaviorChainInvoker behaviorChainInvoker, Metrics metrics,
            Configuration configuration)
        {
            _actionDescriptor = actionDescriptor;
            _behaviorChainInvoker = behaviorChainInvoker;
            _metrics = metrics;
            _configuration = configuration;
            _actionMetrics = metrics.AddAction(_actionDescriptor);
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var result = await _behaviorChainInvoker.Invoke(
                    _actionDescriptor, request, cancellationToken);

                stopwatch.Stop();

                if (_configuration.EnableMetrics)
                {
                    _metrics.IncrementRequests();
                    _actionMetrics.AddRequestTime(stopwatch.Elapsed);
                }

                return result;
            }
            catch (Exception exception)
            {
                if (exception is GraphiteRuntimeInitializationException) throw;
                throw new UnhandledGraphiteRequestException(exception);
            }
        }
    }
}
