using System;
using System.Diagnostics;
using System.Net;
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
        private readonly IBehaviorChainInvoker _behaviorChainInvoker;
        private readonly Metrics _metrics;
        private readonly ActionMetrics _actionMetrics;

        public ActionMessageHandler(Configuration configuration,
            ActionDescriptor actionDescriptor, 
            IBehaviorChainInvoker behaviorChainInvoker, Metrics metrics)
        {
            _actionDescriptor = actionDescriptor;
            _behaviorChainInvoker = behaviorChainInvoker;
            _metrics = metrics;
            _configuration = configuration;
            _actionMetrics = metrics.AddAction(_actionDescriptor);
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var result = await _behaviorChainInvoker.Invoke(
                    _actionDescriptor, requestMessage, cancellationToken);

                stopwatch.Stop();

                if (_configuration.Metrics)
                {
                    _metrics.IncrementRequests();
                    _actionMetrics.AddRequestTime(stopwatch.Elapsed);
                }

                return result;
            }
            catch (Exception exception)
            {
                if (_configuration.ReturnErrorMessage)
                {
                    var responseMessage = requestMessage.CreateResponse(HttpStatusCode.InternalServerError);
                    responseMessage.ReasonPhrase = exception is GraphiteRuntimeInitializationException 
                        ? "Unhandled exception initializing route." 
                        : "Unhandled exception.";
                    responseMessage.Content = new StringContent($"{requestMessage}\r\n\r\n{exception}");
                    return responseMessage;
                }
                if (exception is GraphiteRuntimeInitializationException) throw;
                throw new UnhandledGraphiteRequestException(exception);
            }
        }
    }
}
