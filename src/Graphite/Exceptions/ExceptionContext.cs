using System;
using System.Net.Http;
using Graphite.Actions;
using Graphite.DependencyInjection;

namespace Graphite.Exceptions
{
    public class ExceptionContext
    {
        public ExceptionContext(Exception exception,
            ActionDescriptor actionDescriptor,
            HttpRequestMessage requestMessage,
            IContainer container)
        {
            Exception = exception;
            ActionDescriptor = actionDescriptor;
            RequestMessage = requestMessage;
            Container = container;
        }

        public Exception Exception { get; set; }
        public ActionDescriptor ActionDescriptor { get; set; }
        public HttpRequestMessage RequestMessage { get; set; }
        public IContainer Container { get; set; }
    }
}
