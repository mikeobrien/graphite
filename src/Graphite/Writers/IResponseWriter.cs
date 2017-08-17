using System;
using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Extensibility;

namespace Graphite.Writers
{
    public class ResponseWriterContext
    {
        public ResponseWriterContext(object response)
        {
            Response = response;
        }
        
        public virtual object Response { get; }
    }

    public interface IResponseWriter : IConditional<ResponseWriterContext>
    {
        bool IsWeighted { get; }
        double Weight { get; }
        Task<HttpResponseMessage> Write(ResponseWriterContext context);
    }

    public abstract class ResponseWriterBase : IResponseWriter
    {
        private readonly Configuration _configuration;

        protected ResponseWriterBase(Configuration configuration)
        {
            _configuration = configuration;
        }

        public virtual bool IsWeighted => false;
        public virtual double Weight => 0;

        public virtual bool AppliesTo(ResponseWriterContext context)
        {
            return true;
        }

        public abstract Task<HttpResponseMessage> WriteResponse(
            ResponseWriterContext context);

        public Task<HttpResponseMessage> Write(ResponseWriterContext context)
        {
            return WriteResponse(context);
        }
    }
}