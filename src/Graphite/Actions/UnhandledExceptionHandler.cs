using System;
using System.Net;
using System.Net.Http;
using Graphite.DependencyInjection;

namespace Graphite.Actions
{
    public class UnhandledExceptionHandler : IUnhandledExceptionHandler
    {
        private readonly Configuration _configuration;
        private readonly IContainer _container;

        public UnhandledExceptionHandler(Configuration configuration, IContainer container)
        {
            _configuration = configuration;
            _container = container;
        }

        public HttpResponseMessage HandleException(Exception exception, 
            ActionDescriptor actionDescriptor, HttpRequestMessage requestMessage)
        {
            if (_configuration.ReturnErrorMessage)
            {
                var responseMessage = requestMessage.CreateResponse(HttpStatusCode.InternalServerError);
                responseMessage.ReasonPhrase = _configuration.UnhandledExceptionStatusText;
                var message = $"{actionDescriptor.Action}\r\n{actionDescriptor.Route.Url}\r\n\r\n" +
                              $"{requestMessage}\r\n\r\n{exception}\r\n\r\n{_container.GetConfiguration()}";
                responseMessage.Content = new StringContent(message);
                return responseMessage;
            }
            throw new UnhandledGraphiteRequestException(exception);
        }
    }
}