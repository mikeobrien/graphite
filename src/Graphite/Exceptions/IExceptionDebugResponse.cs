namespace Graphite.Exceptions
{
    public interface IExceptionDebugResponse
    {
        string GetResponse(ExceptionContext context);
    }
}
