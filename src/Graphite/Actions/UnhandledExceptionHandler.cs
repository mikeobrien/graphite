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
                var initializationException = exception as GraphiteRuntimeInitializationException;
                responseMessage.ReasonPhrase = initializationException != null
                    ? "Unable to initialize route."
                    : "Unhandled exception.";
                var containerConfiguration = initializationException?
                    .ContainerConfiguration ?? _container.GetConfiguration();
                var message = $"{actionDescriptor.Action}\r\n{actionDescriptor.Route.Url}\r\n\r\n" +
                              $"{requestMessage}\r\n\r\n{exception}\r\n\r\n{containerConfiguration}";
                responseMessage.Content = new StringContent(message);
                return responseMessage;
            }
            if (exception is GraphiteRuntimeInitializationException) throw exception;
            throw new UnhandledGraphiteRequestException(exception);
        }
    }
}