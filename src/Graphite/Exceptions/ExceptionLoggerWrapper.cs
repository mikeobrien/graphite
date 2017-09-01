using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using Graphite.DependencyInjection;

namespace Graphite.Exceptions
{
    public class ExceptionLoggerWrapper<T> : IExceptionLogger where T : class, IExceptionLogger
    {
        public Task LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken)
        {
            return context.Request.GetGraphiteContainer().GetInstance<T>()
                .LogAsync(context, cancellationToken);
        }
    }
}
