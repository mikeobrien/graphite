using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Extensibility;

namespace Graphite.Writers
{
    public class ResponseWriterContext
    {
        public ResponseWriterContext(object response)
        {
            Response = response;
        }
        
        public virtual object Response { get; }
    }

    public interface IResponseWriter : IConditional<ResponseWriterContext>
    {
        Task<HttpResponseMessage> Write(ResponseWriterContext context);
    }
}