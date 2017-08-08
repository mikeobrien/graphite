using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Graphite.Extensions;

namespace Graphite.Writers
{
    public abstract class WeightedContentWriterBase : ResponseWriterBase
    {
        private readonly HttpResponseMessage _responseMessage;
        private readonly Lazy<AcceptTypeMatch> _acceptType;

        protected WeightedContentWriterBase(HttpRequestMessage requestMessage,
            HttpResponseMessage responseMessage, params string[] mimeTypes)
        {
            _responseMessage = responseMessage;
            _acceptType = requestMessage.ToLazy(x => x
                .GetFirstMatchingAcceptTypeOrDefault(mimeTypes));
        }

        protected abstract HttpContent GetContent(ResponseWriterContext context);

        public override bool IsWeighted => true;
        public override double Weight => _acceptType.Value?.GetWeight() ?? 0;

        public override bool AppliesTo(ResponseWriterContext context)
        {
            return _acceptType.Value != null;
        }

        public override Task<HttpResponseMessage> Write(ResponseWriterContext context)
        {
            _responseMessage.Content = GetContent(context);
            if (_responseMessage.Content != null)
                _responseMessage.Content.Headers.ContentType = new
                    MediaTypeHeaderValue(_acceptType.Value.ContentType);
            return _responseMessage.ToTaskResult();
        }
    }
}