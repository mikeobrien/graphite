using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Graphite.Writers
{
    public abstract class PushWriterBase : WeightedContentWriterBase
    {
        private readonly Configuration _configuration;

        protected PushWriterBase(HttpRequestMessage requestMessage,
            HttpResponseMessage responseMessage, Configuration configuration,
            params string[] mimeTypes)
            : base(requestMessage, responseMessage, mimeTypes)
        {
            _configuration = configuration;
        }

        protected abstract Task WriteResponse(ResponseWriterContext context,
            Stream stream, TransportContext transportContext);

        protected override HttpContent GetContent(ResponseWriterContext context)
        {
            return new PushContent((s, tc) =>
            {
                try
                {
                    return WriteResponse(context, s, tc);
                }
                finally
                {
                    if (_configuration.DisposeResponses)
                        (context.Response as IDisposable)?.Dispose();
                }
            });
        }
    }
}