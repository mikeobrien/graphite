using System.Threading.Tasks;
using Graphite.Actions;
using Graphite.Extensibility;

namespace Graphite.Readers
{
    public class RequestReaderContext
    {
        public RequestReaderContext(Configuration configuration, 
            RequestContext requestContext)
        {
            Configuration = configuration;
            RequestContext = requestContext;
        }

        public virtual Configuration Configuration { get; }
        public virtual RequestContext RequestContext { get; }
    }

    public interface IRequestReader : IConditional<RequestReaderContext>
    {
        Task<object> Read(RequestReaderContext context);
    }
}