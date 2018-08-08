using System.Net.Http;
using Graphite.Extensibility;

namespace Graphite.Actions
{
    public class ResponseHeadersContext
    {
        public ResponseHeadersContext(HttpResponseMessage responseMessage)
        {
            ResponseMessage = responseMessage;
        }
        
        public virtual HttpResponseMessage ResponseMessage { get; }
    }

    public interface IResponseHeaders : IConditional<ResponseHeadersContext>
    {
        void SetHeaders(ResponseHeadersContext context);
    }
}
