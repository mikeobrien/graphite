using System.Threading.Tasks;
using Graphite.Extensibility;

namespace Graphite.Binding
{
    public class RequestBinderContext
    {
        public RequestBinderContext(object[] actionArguments)
        {
            ActionArguments = actionArguments;
        }
        public virtual object[] ActionArguments { get; }
    }

    public interface IRequestBinder : IConditional<RequestBinderContext>
    {
        Task<BindResult> Bind(RequestBinderContext context);
    }
}