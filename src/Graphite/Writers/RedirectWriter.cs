using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Extensions;

namespace Graphite.Writers
{
    public enum RedirectType
    {
        None = 0,
        MovedPermanently = 301,
        Found = 302,
        SeeOther = 303,
        NotModified = 304,
        UseProxy = 305,
        TemporaryKeepVerb = 307,
        PermanentKeepVerb = 308
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
        public virtual bool AppliesTo(ResponseWriterContext context)
        {
            var responseType = context.RequestContext.Route.ResponseType?.Type;
            if (!context.RequestContext.Route.HasResponse ||
                !typeof(IRedirectable).IsAssignableFrom(responseType)) return false;
            var redirect = context.Response.As<IRedirectable>();
            return redirect.Ridirect != RedirectType.None && redirect.RidirectUrl.IsNotNullOrEmpty();
        }

        public Task<HttpResponseMessage> Write(ResponseWriterContext context)
        {
            var redirect = context.Response.As<IRedirectable>();
            return new HttpResponseMessage
            {
                Headers =
                {
                    Location = new Uri(redirect.RidirectUrl)
                },
                StatusCode = (HttpStatusCode)redirect.Ridirect
            }.ToTaskResult();
        }
    }
}