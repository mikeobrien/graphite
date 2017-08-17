using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Extensions;
using Graphite.Http;

namespace Graphite.Writers
{
    public abstract class SerializerWriterBase : WeightedContentWriterBase
    {
        private readonly Configuration _configuration;

        protected SerializerWriterBase(
            HttpRequestMessage requestMessage,
            HttpResponseMessage responseMessage,
            Configuration configuration,
            params string[] mimeTypes) :
            base(requestMessage, responseMessage,
                configuration, mimeTypes)
        {
            _configuration = configuration;
        }

        protected abstract void WriteToStream(ResponseWriterContext context, Stream output);

        protected sealed override HttpContent GetContent(ResponseWriterContext context)
        {
            return new AsyncContent(output =>
            {
                try
                {
                    WriteToStream(context, output);
                }
                finally
                {
                    if (_configuration.DisposeSerializedObjects)
                        context.Response.As<IDisposable>()?.Dispose();
                }
                return Task.CompletedTask;
            });
        }
    }
}