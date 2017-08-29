using System;
using System.Net;
using System.Net.Http;
using Graphite.Extensibility;

namespace Graphite.Actions
{
    public enum ResponseState
    {
        BindingFailure, NoReader, HasResponse, NoResponse, NoWriter
    }

    public class ResponseStatusContext
    {
        public ResponseStatusContext(HttpResponseMessage responseMessage, 
            ResponseState responseState, string errorMessage)
        {
            ResponseMessage = responseMessage;
            ResponseState = responseState;
            ErrorMessage = errorMessage;
        }
        
        public virtual HttpResponseMessage ResponseMessage { get; }
        public virtual ResponseState ResponseState { get; }
        public virtual string ErrorMessage { get; }
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

    public class BindingFailureStatusAttribute : ResponseStatusAttributeBase
    {
        public BindingFailureStatusAttribute(HttpStatusCode statusCode) : base(statusCode) { }

        public BindingFailureStatusAttribute(string statusText) : base(statusText) { }
    }

    public class NoReaderStatusAttribute : ResponseStatusAttributeBase
    {
        public NoReaderStatusAttribute(HttpStatusCode statusCode,
            string statusText = null) : base(statusCode, statusText) { }

        public NoReaderStatusAttribute(string statusText) : base(statusText) { }
    }

    public class HasResponseStatusAttribute : ResponseStatusAttributeBase
    {
        public HasResponseStatusAttribute(HttpStatusCode statusCode, 
            string statusText = null) : base(statusCode, statusText) { }

        public HasResponseStatusAttribute(string statusText) : base(statusText) { }
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
