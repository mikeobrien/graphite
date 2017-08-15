using System.Net;
using System.Net.Http;
using Graphite.Extensions;

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
                case ResponseState.Response:
                    SetStatus<ResponseStatusAttribute>(context.ResponseMessage,
                        _configuration.DefaultResponseStatusCode,
                        _configuration.DefaultResponseStatusText);
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
            if ((statusAttribute?.StatusText).IsNotNullOrEmpty())
                responseMessage.ReasonPhrase = statusAttribute.StatusText;
            else if (defaultStatusText.IsNotNullOrEmpty())
                responseMessage.ReasonPhrase = defaultStatusText;
        }
    }
}