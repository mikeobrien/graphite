using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Extensions;

namespace Graphite.Writers
{
    public class HttpResponseMessageWriter : IResponseWriter
    {
        public bool AppliesTo(ResponseWriterContext context)
        {
            return context.Response is HttpResponseMessage;
        }

        public Task<HttpResponseMessage> Write(ResponseWriterContext context)
        {
            return context.Response.As<HttpResponseMessage>().ToTaskResult();
        }
    }
}