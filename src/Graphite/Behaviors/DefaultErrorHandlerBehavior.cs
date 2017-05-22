using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Binding;

namespace Graphite.Behaviors
{
    public class DefaultErrorHandlerBehavior : BehaviorBase
    {
        private readonly Configuration _configuration;
        private readonly HttpRequestMessage _request;

        public DefaultErrorHandlerBehavior(Configuration configuration,
            IBehavior innerBehavior, HttpRequestMessage request) : base(innerBehavior)
        {
            _configuration = configuration;
            _request = request;
        }

        public override async Task<HttpResponseMessage> Invoke()
        {
            try
            {
                return await InnerBehavior.Invoke();
            }
            catch (BadRequestException exception)
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    ReasonPhrase = exception.Message
                };
            }
            catch (Exception exception)
            {
                var message = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    ReasonPhrase = _configuration.UnhandledExceptionStatusText
                };
                if (_configuration.EnableDiagnostics) message
                    .Content = new StringContent($"{_request}\r\n\r\n{exception}");
                return message;
            }
        }
    }
}
