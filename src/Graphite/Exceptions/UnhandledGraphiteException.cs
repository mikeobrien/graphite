using System;
using System.Net.Http;
using Graphite.Actions;
using Graphite.DependencyInjection;

namespace Graphite.Exceptions
{
    public class UnhandledGraphiteException : Exception
    {
        public UnhandledGraphiteException(ActionDescriptor actionDescriptor,
            HttpRequestMessage requestMessage, IContainer container, Exception exception) : 
            base("An unhandled Graphite exception has occured.", exception)
        {
            ActionDescriptor = actionDescriptor;
            RequestMessage = requestMessage;
            Container = container;
        }

        public UnhandledGraphiteException(ExceptionContext context) : 
            this(context.ActionDescriptor, context.RequestMessage, 
                context.Container, context.Exception) { }
        
        public ActionDescriptor ActionDescriptor { get; set; }
        public HttpRequestMessage RequestMessage { get; set; }
        public IContainer Container { get; set; }
    }
}