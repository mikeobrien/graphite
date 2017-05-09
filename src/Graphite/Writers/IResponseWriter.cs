using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Actions;
using Graphite.Extensibility;

namespace Graphite.Writers
{
    public class ResponseWriterContext
    {
        public ResponseWriterContext(Configuration configuration, 
            RequestContext requestContext, object response)
        {
            Configuration = configuration;
            RequestContext = requestContext;
            Response = response;
        }

        public virtual Configuration Configuration { get; }
        public virtual RequestContext RequestContext { get; }
        public virtual object Response { get; }
    }

    public interface IResponseWriter : IConditional<ResponseWriterContext>
    {
        Task<HttpResponseMessage> Write(ResponseWriterContext context);
    }
}