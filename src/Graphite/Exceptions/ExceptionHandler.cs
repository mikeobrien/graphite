using System.Net;
using System.Net.Http;
using System.Text;
using Graphite.Actions;
using Graphite.Http;

namespace Graphite.Exceptions
{
    public class ExceptionHandler : IExceptionHandler
    {
        private readonly Configuration _configuration;
        private readonly IExceptionDebugResponse _exceptionResponse;

        public ExceptionHandler(Configuration configuration, 
            IExceptionDebugResponse exceptionResponse)
        {
            _configuration = configuration;
            _exceptionResponse = exceptionResponse;
        }

        public HttpResponseMessage HandleException(ExceptionContext context)
        {
            if (_configuration.ReturnErrorMessage(context.RequestMessage))
            {
                var unhandledResponse = context.RequestMessage
                    .CreateResponse(HttpStatusCode.InternalServerError);
                unhandledResponse.ReasonPhrase = _configuration.UnhandledExceptionStatusText;
                unhandledResponse.Content = new StringContent(
                    _exceptionResponse.GetResponse(
                        context.Exception, context.ActionDescriptor, 
                        context.RequestMessage, context.Container), 
                    Encoding.UTF8, MimeTypes.TextPlain);
                return unhandledResponse;
            }

            throw new UnhandledGraphiteException(context);
        }
    }
}