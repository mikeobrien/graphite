using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using Graphite.DependencyInjection;

namespace Graphite.Exceptions
{
    public class ExceptionHandlerWrapper<T> : IExceptionHandler where T : class, IExceptionHandler
    {
        public Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            return context.Request.GetGraphiteContainer().GetInstance<T>()
                .HandleAsync(context, cancellationToken);
        }
    }
}
