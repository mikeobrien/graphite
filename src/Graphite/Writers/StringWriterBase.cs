using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Graphite.Extensions;

namespace Graphite.Writers
{
    public abstract class StringWriterBase : IResponseWriter
    {
        private readonly string _mimeType;
        private readonly Encoding _encoding;

        protected StringWriterBase(string mimeType, Encoding encoding)
        {
            _mimeType = mimeType;
            _encoding = encoding;
        }

        protected abstract string GetResponse(ResponseWriterContext context);

        public virtual bool AppliesTo(ResponseWriterContext context)
        {
            return context.RequestContext.Route.HasResponse && context
                .RequestContext.RequestMessage.AcceptsMimeType(_mimeType);
        }

        public Task<HttpResponseMessage> Write(ResponseWriterContext context)
        {
            var response = new HttpResponseMessage();
            var data = GetResponse(context);
            if (data != null)
                response.Content = new StringContent(data, _encoding, _mimeType);
            return response.ToTaskResult();
        }
    }
}