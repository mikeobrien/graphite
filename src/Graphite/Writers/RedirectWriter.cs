using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Extensions;
using Graphite.Routing;

namespace Graphite.Writers
{
    public enum RedirectType
    {
        None = 0,
        /// <summary>
        /// 301 This response code means that URI of requested resource 
        /// has been changed. Probably, new URI would be given in the response.
        /// </summary>
        MovedPermanently = 301,
        /// <summary>
        /// 302 This response code means that URI of requested resource has 
        /// been changed temporarily. New changes in the URI might be made 
        /// in the future. Therefore, this same URI should be used by the 
        /// client in future requests.
        /// </summary>
        Found = 302,
        /// <summary>
        /// 303 Server sent this response to directing client to get 
        /// requested resource to another URI with an GET request.
        /// </summary>
        SeeOther = 303,
        /// <summary>
        /// 304 This is used for caching purposes. It is telling to client that 
        /// response has not been modified. So, client can continue to 
        /// use same cached version of response.
        /// </summary>
        NotModified = 304,
        /// <summary>
        /// 305 DEPRICATED Was defined in a previous version of the HTTP specification 
        /// to indicate that a requested response must be accessed by a proxy. It has 
        /// been deprecated due to security concerns regarding in-band 
        /// configuration of a proxy.
        /// </summary>
        UseProxy = 305,
        /// <summary>
        /// 307	Server sent this response to directing client to get requested resource 
        /// to another URI with same method that used prior request. This has the same 
        /// semantic than the 302 Found HTTP response code, with the exception that the 
        /// user agent must not change the HTTP method used: if a POST was used in 
        /// the first request, a POST must be used in the second request.
        /// </summary>
        TemporaryRedirect = 307,
        /// <summary>
        /// 308	This means that the resource is now permanently located at another URI, 
        /// specified by the Location: HTTP Response header. This has the same semantics 
        /// as the 301 Moved Permanently HTTP response code, with the exception that the 
        /// user agent must not change the HTTP method used: if a POST was used in 
        /// the first request, a POST must be used in the second request.
        /// </summary>
        PermanentRedirect = 308
    }

    public interface IRedirectable
    {
        RedirectType Ridirect { get; }
        string RidirectUrl { get; }
    }

    public class Redirect : IRedirectable
    {
        public string Url { get; set; }
        public RedirectType Type { get; set; } = RedirectType.MovedPermanently;

        string IRedirectable.RidirectUrl => Url;
        RedirectType IRedirectable.Ridirect => Type;
    }
    
    public class RedirectWriter : IResponseWriter
    {
        private readonly RouteDescriptor _routeDescriptor;
        private readonly HttpResponseMessage _responseMessage;

        public RedirectWriter(RouteDescriptor routeDescriptor,
            HttpResponseMessage responseMessage)
        {
            _routeDescriptor = routeDescriptor;
            _responseMessage = responseMessage;
        }

        public virtual bool AppliesTo(ResponseWriterContext context)
        {
            var responseType = _routeDescriptor.ResponseType?.Type;
            if (!typeof(IRedirectable).IsAssignableFrom(responseType)) return false;
            var redirect = context.Response.As<IRedirectable>();
            return redirect.Ridirect != RedirectType.None && redirect.RidirectUrl.IsNotNullOrEmpty();
        }

        public Task<HttpResponseMessage> Write(ResponseWriterContext context)
        {
            var redirect = context.Response.As<IRedirectable>();
            _responseMessage.StatusCode = (HttpStatusCode) redirect.Ridirect;
            _responseMessage.Headers.Location = new Uri(redirect.RidirectUrl, UriKind.RelativeOrAbsolute);
            return _responseMessage.ToTaskResult();
        }
    }
}