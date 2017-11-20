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
        protected ResponseStatusAttributeBase(HttpStatusCode statusCode, string reasonPhrase = null)
        {
            StatusCode = statusCode;
            ReasonPhrase = reasonPhrase;
        }

        protected ResponseStatusAttributeBase(string reasonPhrase)
        {
            ReasonPhrase = reasonPhrase;
        }

        public HttpStatusCode? StatusCode { get; }
        public string ReasonPhrase { get; }
    }

    public class BindingFailureStatusAttribute : ResponseStatusAttributeBase
    {
        public BindingFailureStatusAttribute(HttpStatusCode statusCode) : base(statusCode) { }

        public BindingFailureStatusAttribute(string reasonPhrase) : base(reasonPhrase) { }
    }

    public class NoReaderStatusAttribute : ResponseStatusAttributeBase
    {
        public NoReaderStatusAttribute(HttpStatusCode statusCode,
            string reasonPhrase = null) : base(statusCode, reasonPhrase) { }

        public NoReaderStatusAttribute(string reasonPhrase) : base(reasonPhrase) { }
    }

    public class HasResponseStatusAttribute : ResponseStatusAttributeBase
    {
        public HasResponseStatusAttribute(HttpStatusCode statusCode, 
            string reasonPhrase = null) : base(statusCode, reasonPhrase) { }

        public HasResponseStatusAttribute(string reasonPhrase) : base(reasonPhrase) { }
    }

    public class NoResponseStatusAttribute : ResponseStatusAttributeBase
    {
        public NoResponseStatusAttribute(HttpStatusCode statusCode,
            string reasonPhrase = null) : base(statusCode, reasonPhrase) { }

        public NoResponseStatusAttribute(string reasonPhrase) : base(reasonPhrase) { }
    }

    public class NoWriterStatusAttribute : ResponseStatusAttributeBase
    {
        public NoWriterStatusAttribute(HttpStatusCode statusCode,
            string reasonPhrase = null) : base(statusCode, reasonPhrase) { }

        public NoWriterStatusAttribute(string reasonPhrase) : base(reasonPhrase) { }
    }
}
