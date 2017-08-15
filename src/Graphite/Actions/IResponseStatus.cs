using System;
using System.Net;
using System.Net.Http;
using Graphite.Extensibility;

namespace Graphite.Actions
{
    public enum ResponseState
    {
        Response, NoResponse, NoWriter
    }

    public class ResponseStatusContext
    {
        public ResponseStatusContext(HttpResponseMessage responseMessage, 
            ResponseState responseState)
        {
            ResponseMessage = responseMessage;
            ResponseState = responseState;
        }
        
        public HttpResponseMessage ResponseMessage { get; }
        public virtual ResponseState ResponseState { get; }
    }

    public interface IResponseStatus : IConditional<ResponseStatusContext>
    {
        void SetStatus(ResponseStatusContext context);
    }

    public abstract class ResponseStatusAttributeBase : Attribute
    {
        protected ResponseStatusAttributeBase(HttpStatusCode statusCode, string statusText = null)
        {
            StatusCode = statusCode;
            StatusText = statusText;
        }

        protected ResponseStatusAttributeBase(string statusText)
        {
            StatusText = statusText;
        }

        public HttpStatusCode? StatusCode { get; }
        public string StatusText { get; }
    }

    public class ResponseStatusAttribute : ResponseStatusAttributeBase
    {
        public ResponseStatusAttribute(HttpStatusCode statusCode, 
            string statusText = null) : base(statusCode, statusText) { }

        public ResponseStatusAttribute(string statusText) : base(statusText) { }
    }

    public class NoResponseStatusAttribute : ResponseStatusAttributeBase
    {
        public NoResponseStatusAttribute(HttpStatusCode statusCode,
            string statusText = null) : base(statusCode, statusText) { }

        public NoResponseStatusAttribute(string statusText) : base(statusText) { }
    }

    public class NoWriterStatusAttribute : ResponseStatusAttributeBase
    {
        public NoWriterStatusAttribute(HttpStatusCode statusCode,
            string statusText = null) : base(statusCode, statusText) { }

        public NoWriterStatusAttribute(string statusText) : base(statusText) { }
    }
}
