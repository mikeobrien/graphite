using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Graphite.Extensions;

namespace Graphite.Writers
{
    public abstract class StringWriterBase : IResponseWriter
    {
        private readonly HttpRequestMessage _requestMessage;
        private readonly HttpResponseMessage _responseMessage;
        private readonly string _mimeType;
        private readonly Encoding _encoding;

        protected StringWriterBase(HttpRequestMessage requestMessage, 
            HttpResponseMessage responseMessage, string mimeType, Encoding encoding)
        {
            _requestMessage = requestMessage;
            _responseMessage = responseMessage;
            _mimeType = mimeType;
            _encoding = encoding;
        }

        protected abstract string GetResponse(ResponseWriterContext context);

        public virtual bool AppliesTo(ResponseWriterContext context)
        {
            return _requestMessage.AcceptsMimeType(_mimeType);
        }

        public Task<HttpResponseMessage> Write(ResponseWriterContext context)
        {
            var data = GetResponse(context);
            if (data != null)
            {
                _responseMessage.Content = new AsyncStringContent(data, _encoding);
                if (_mimeType.IsNotNullOrEmpty())
                    _responseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(_mimeType);
            }
            return _responseMessage.ToTaskResult();
        }
    }
}