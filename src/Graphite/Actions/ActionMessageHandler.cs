using System;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Graphite.Behaviors;
using Graphite.DependencyInjection;
using Graphite.Exceptions;
using Graphite.Monitoring;

namespace Graphite.Actions
{
    public class ActionMessageHandler : HttpMessageHandler
    {
        private readonly Configuration _configuration;
        private readonly ActionDescriptor _actionDescriptor;
        private readonly IBehaviorChainInvoker _behaviorChainInvoker;
        private readonly IContainer _container;
        private readonly Metrics _metrics;
        private readonly ActionMetrics _actionMetrics;

        public ActionMessageHandler(Configuration configuration,
            ActionDescriptor actionDescriptor,
            IBehaviorChainInvoker behaviorChainInvoker,
            IContainer container, Metrics metrics)
        {
            _configuration = configuration;
            _actionDescriptor = actionDescriptor;
            _behaviorChainInvoker = behaviorChainInvoker;
            _container = container;
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
                if (exception is UnhandledGraphiteException) throw;
                throw new UnhandledGraphiteException(_actionDescriptor, 
                    _container, exception);
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
