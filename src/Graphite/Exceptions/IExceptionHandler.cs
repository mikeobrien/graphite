using System.Net.Http;

namespace Graphite.Exceptions
{
    public interface IExceptionHandler
    {
        HttpResponseMessage HandleException(ExceptionContext context);
    }
}
