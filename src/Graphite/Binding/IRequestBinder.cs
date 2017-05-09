using System.Threading.Tasks;
using Graphite.Actions;
using Graphite.Extensibility;

namespace Graphite.Binding
{
    public class RequestBinderContext
    {
        public RequestBinderContext(Configuration configuration, 
            RequestContext requestContext, object[] actionArguments)
        {
            Configuration = configuration;
            RequestContext = requestContext;
            ActionArguments = actionArguments;
        }

        public virtual Configuration Configuration { get; }
        public virtual RequestContext RequestContext { get; }
        public virtual object[] ActionArguments { get; }
    }

    public interface IRequestBinder : IConditional<RequestBinderContext>
    {
        Task Bind(RequestBinderContext context);
    }
}