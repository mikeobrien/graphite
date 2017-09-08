using System;
using System.Linq;
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
        private readonly string[] _mimeTypes;

        protected WeightedContentWriterBase(HttpRequestMessage requestMessage,
            HttpResponseMessage responseMessage, params string[] mimeTypes)
        {
            _responseMessage = responseMessage;
            _mimeTypes = mimeTypes;
            _acceptType = requestMessage.ToLazy(x => x
                .GetMatchingAcceptTypes(mimeTypes).FirstOrDefault());
        }

        protected abstract HttpContent GetContent(ResponseWriterContext context);

        public override bool IsWeighted => true;
        public override double Weight => _acceptType.Value?.GetWeight() ?? 0;

        public override bool AppliesTo(ResponseWriterContext context)
        {
            return _acceptType.Value != null;
        }

        public override Task<HttpResponseMessage> WriteResponse(ResponseWriterContext context)
        {
            _responseMessage.Content = GetContent(context);
            if (_responseMessage.Content != null)
            {
                var contentType = _acceptType.Value?.ContentType ?? _mimeTypes.First();
                if (contentType.IsNotNullOrEmpty())
                    _responseMessage.Content.Headers.ContentType = new
                        MediaTypeHeaderValue(contentType);
            }
            return _responseMessage.ToTaskResult();
        }
    }
}