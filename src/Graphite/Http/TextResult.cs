using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Graphite.Http
{
    public class TextResult : IHttpActionResult
    {
        private readonly HttpRequestMessage _request;
        private readonly string _data;
        private readonly HttpStatusCode _status;
        private readonly string _reasonPhrase;

        public TextResult(HttpRequestMessage request, string data,
            HttpStatusCode status, string reasonPhrase) :
            this(request, status, reasonPhrase)
        {
            _data = data;
        }

        public TextResult(HttpRequestMessage request,
            HttpStatusCode status, string reasonPhrase)
        {
            _data = "";
            _request = request;
            _status = status;
            _reasonPhrase = reasonPhrase;
        }

        public Task<HttpResponseMessage> ExecuteAsync(
            CancellationToken cancellationToken)
        {
            var response = _request.CreateResponse(_status);
            response.SafeSetReasonPhrase(_reasonPhrase);
            response.Content = new StringContent(_data);
            response.Content.Headers.ContentType =
                new MediaTypeHeaderValue(MimeTypes.TextPlain)
                {
                    CharSet = "utf-8"
                };
            return Task.FromResult(response);
        }
    }
}
