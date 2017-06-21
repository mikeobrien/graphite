using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Graphite.Extensions;

namespace Graphite.Writers
{
    public abstract class StringWriterBase : ResponseWriterBase
    {
        private readonly HttpResponseMessage _responseMessage;
        private readonly Lazy<AcceptTypeMatch> _acceptType;
        private readonly Encoding _encoding;

        protected StringWriterBase(HttpRequestMessage requestMessage, 
            HttpResponseMessage responseMessage, Encoding encoding, params string[] mimeTypes)
        {
            _responseMessage = responseMessage;
            _acceptType = requestMessage.ToLazy(x => x
                .GetFirstMatchingAcceptTypeOrDefault(mimeTypes));
            _encoding = encoding;
        }

        protected abstract string GetResponse(ResponseWriterContext context);

        public override bool IsWeighted => true;
        public override double Weight => _acceptType.Value?.GetWeight() ?? 0;

        public override bool AppliesTo(ResponseWriterContext context)
        {
            return _acceptType.Value != null;
        }

        public override Task<HttpResponseMessage> Write(ResponseWriterContext context)
        {
            var data = GetResponse(context);
            if (data != null)
            {
                _responseMessage.Content = new AsyncStringContent(data, _encoding);
                if (_acceptType.Value != null)
                    _responseMessage.Content.Headers.ContentType = new 
                        MediaTypeHeaderValue(_acceptType.Value.ContentType);
            }
            return _responseMessage.ToTaskResult();
        }
    }
}