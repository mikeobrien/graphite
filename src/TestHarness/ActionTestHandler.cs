using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Graphite.Actions;
using Graphite.Extensions;
using Graphite.Writers;

namespace TestHarness.Action
{
    public class ActionTestHandler
    {
        private readonly HttpResponseHeaders _headers;

        public ActionTestHandler(HttpResponseHeaders headers)
        {
            _headers = headers;
        }
        public void GetUpdateHeaders()
        {
            _headers.Add("fark", "farker");
        }

        public void GetUpdateCookies()
        {
            _headers.SetCookie("fark", "farker");
        }

        public HttpResponseMessage GetWithResponseMessage()
        {
            return new HttpResponseMessage(HttpStatusCode.PaymentRequired);
        }

        public string GetWithInteceptor()
        {
            return "fark";
        }

        public class Interceptor : IInterceptor
        {
            public bool AppliesTo(InterceptorContext context)
            {
                return true;
            }

            public Task<HttpResponseMessage> Intercept(InterceptorContext context)
            {
                var response = context.RequestMessage.CreateResponse();
                response.Content = new AsyncStringContent("farker", Encoding.UTF8);
                return response.ToTaskResult();
            }
        }
    }
}