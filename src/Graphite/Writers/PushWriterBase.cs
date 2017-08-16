using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Graphite.Writers
{
    public abstract class PushWriterBase : WeightedContentWriterBase
    {
        protected PushWriterBase(HttpRequestMessage requestMessage,
            HttpResponseMessage responseMessage, 
            Configuration configuration, 
            params string[] mimeTypes) :
            base(requestMessage, responseMessage, 
                configuration, mimeTypes) { }

        protected abstract Task WriteResponse(ResponseWriterContext context,
            Stream stream, TransportContext transportContext);

        protected override HttpContent GetContent(ResponseWriterContext context)
        {
            return new PushContent((s, tc) => WriteResponse(context, s, tc));
        }
    }
}