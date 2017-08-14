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
        private readonly HttpRequestMessage _requestMessage;
        private readonly HttpResponseMessage _responseMessage;

        public DefaultErrorHandlerBehavior(Configuration configuration,
            IBehaviorChain behaviorChain, HttpRequestMessage requestMessage,
            HttpResponseMessage responseMessage) : base(behaviorChain)
        {
            _configuration = configuration;
            _requestMessage = requestMessage;
            _responseMessage = responseMessage;
        }

        public override async Task<HttpResponseMessage> Invoke()
        {
            try
            {
                return await BehaviorChain.InvokeNext();
            }
            catch (BadRequestException exception)
            {
                _responseMessage.StatusCode = HttpStatusCode.BadRequest;
                _responseMessage.ReasonPhrase = exception.Message;
            }
            catch (Exception exception)
            {
                _responseMessage.StatusCode = HttpStatusCode.InternalServerError;
                _responseMessage.ReasonPhrase = _configuration.UnhandledExceptionStatusText;

                if (_configuration.ReturnErrorMessage(_requestMessage)) _responseMessage
                    .Content = new StringContent($"{_requestMessage}\r\n\r\n{exception}");
            }
            return _responseMessage;
        }
    }
}
