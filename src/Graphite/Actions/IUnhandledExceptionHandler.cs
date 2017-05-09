using System;
using System.Net.Http;

namespace Graphite.Actions
{
    public interface IUnhandledExceptionHandler
    {
        HttpResponseMessage HandleException(Exception exception, 
            ActionDescriptor actionDescriptor, HttpRequestMessage requestMessage);
    }
}
