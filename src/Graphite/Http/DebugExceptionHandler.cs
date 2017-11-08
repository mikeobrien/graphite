using System.Net;
using System.Web.Http.ExceptionHandling;
using Graphite.Exceptions;

namespace Graphite.Http
{
    public class DebugExceptionHandler : ExceptionHandler
    {
        private readonly Configuration _configuration;

        public DebugExceptionHandler(Configuration configuration)
        {
            _configuration = configuration;
        }

        public override void Handle(ExceptionHandlerContext context)
        {
            if (_configuration.Diagnostics)
                context.Result = new TextResult(context.Request,
                    context.Exception.GetDebugResponse(context.Request),
                    HttpStatusCode.InternalServerError,
                    "An unhandled exception has occured.");
        }
    }
}
