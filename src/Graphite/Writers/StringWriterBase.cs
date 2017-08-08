using System.Net.Http;
using System.Text;

namespace Graphite.Writers
{
    public abstract class StringWriterBase : WeightedContentWriterBase
    {
        private readonly Encoding _encoding;

        protected StringWriterBase(HttpRequestMessage requestMessage,
            HttpResponseMessage responseMessage, Encoding encoding, 
            params string[] mimeTypes)
            : base(requestMessage, responseMessage, mimeTypes)
        {
            _encoding = encoding;
        }

        protected abstract string GetResponse(ResponseWriterContext context);

        protected override HttpContent GetContent(ResponseWriterContext context)
        {
            return new AsyncStringContent(GetResponse(context), _encoding);
        }
    }
}