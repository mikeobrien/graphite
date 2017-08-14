using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Graphite.Extensions;

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

        public HttpResponseMessage GetException()
        {
            throw new Exception("fark");
        }
    }
}