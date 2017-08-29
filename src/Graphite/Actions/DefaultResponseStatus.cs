using System.Net;
using System.Net.Http;

namespace Graphite.Actions
{
    public class DefaultResponseStatus : IResponseStatus
    {
        private readonly Configuration _configuration;
        private readonly ActionMethod _actionMethod;

        public DefaultResponseStatus(Configuration configuration, ActionMethod actionMethod)
        {
            _configuration = configuration;
            _actionMethod = actionMethod;
        }

        public virtual bool AppliesTo(ResponseStatusContext context)
        {
            return true;
        }

        public virtual void SetStatus(ResponseStatusContext context)
        {
            switch (context.ResponseState)
            {
                case ResponseState.NoReader:
                    SetStatus<NoReaderStatusAttribute>(context.ResponseMessage,
                        _configuration.DefaultNoReaderStatusCode,
                        _configuration.DefaultNoReaderStatusText);
                    break;
                case ResponseState.BindingFailure:
                    SetStatus<BindingFailureStatusAttribute>(context.ResponseMessage,
                        _configuration.DefaultBindingFailureStatusCode,
                        _configuration.DefaultBindingFailureStatusText?
                            .Invoke(context.ErrorMessage));
                    break;
                case ResponseState.HasResponse:
                    SetStatus<HasResponseStatusAttribute>(context.ResponseMessage,
                        _configuration.DefaultHasResponseStatusCode,
                        _configuration.DefaultHasResponseStatusText);
                    break;
                case ResponseState.NoResponse:
                    SetStatus<NoResponseStatusAttribute>(context.ResponseMessage, 
                        _configuration.DefaultNoResponseStatusCode,
                        _configuration.DefaultNoResponseStatusText);
                    break;
                case ResponseState.NoWriter:
                    SetStatus<NoWriterStatusAttribute>(context.ResponseMessage,
                        _configuration.DefaultNoWriterStatusCode,
                        _configuration.DefaultNoWriterStatusText);
                    break;
            }
        }

        private void SetStatus<T>(HttpResponseMessage responseMessage,
            HttpStatusCode defaultStatus, string defaultStatusText)
            where T : ResponseStatusAttributeBase
        {
            var statusAttribute = _actionMethod.GetActionOrHandlerAttribute<T>();
            responseMessage.StatusCode = statusAttribute?.StatusCode ?? defaultStatus;

            var statusText = statusAttribute?.StatusText ?? defaultStatusText;
            if (statusText != null) responseMessage.ReasonPhrase = statusText;
        }
    }
}