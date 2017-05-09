using System;
using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Actions;
using Graphite.Extensions;
using Graphite.Routing;

namespace Graphite.Writers
{
    public interface IOutputInfo
    {
        string Filename { get; }
        string ContentType { get; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public abstract class OutputInfoAttribute : Attribute, IOutputInfo
    {
        protected OutputInfoAttribute(string contentType = null,
            string filename = null)
        {
            ContentType = contentType;
            Filename = filename;
        }

        public string Filename { get; }
        public string ContentType { get; }
    }

    public abstract class OutputBody<T> : IOutputInfo
    {
        public string Filename { get; set; }
        public string ContentType { get; set; }
        public T Data { get; set; }
    }

    public abstract class BodyWriterBase<T, TWrapper, TAttribute, TOutputInfo> : IResponseWriter
        where TWrapper : OutputBody<T>, new() 
        where TAttribute : Attribute, TOutputInfo
        where TOutputInfo : class, IOutputInfo
        where T : class
    {
        private readonly ActionMethod _actionMethod;
        private readonly RouteDescriptor _routeDescriptor;
        private readonly HttpResponseMessage _responseMessage;

        protected BodyWriterBase(ActionMethod actionMethod, 
            RouteDescriptor routeDescriptor, 
            HttpResponseMessage responseMessage)
        {
            _routeDescriptor = routeDescriptor;
            _responseMessage = responseMessage;
            _actionMethod = actionMethod;
        }

        public virtual bool AppliesTo(ResponseWriterContext context)
        {
            var responseType = _routeDescriptor.ResponseType?.Type;
            return responseType == typeof(TWrapper) || responseType == typeof(T);
        }

        protected abstract HttpContent GetContent(T data, TOutputInfo outputInfo);
        protected abstract string GetContentType(T data);

        public Task<HttpResponseMessage> Write(ResponseWriterContext context)
        {
            var outputInfo = context.Response.As<TOutputInfo>() ?? 
                _actionMethod.MethodDescriptor.GetAttribute<TAttribute>();
            var data = context.Response.As<OutputBody<T>>()?.Data ?? context.Response as T;

            if (data != null)
            {
                _responseMessage.Content = GetContent(data, outputInfo);
                _responseMessage.Content.Headers.SetContentType(
                    outputInfo?.ContentType ?? GetContentType(data));
                if (outputInfo != null && outputInfo.Filename.IsNotNullOrEmpty())
                    _responseMessage.Content.Headers.SetAttachmentDisposition(outputInfo.Filename);
            }
            return _responseMessage.ToTaskResult();
        }
    }
}
