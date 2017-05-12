using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Extensions;
using Graphite.Http;

namespace Graphite.Writers
{
    public interface IStreamInfo
    {
        string Filename { get; }
        string ContentType { get; }
        int? BufferSize { get; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class OutputStreamAttribute : Attribute, IStreamInfo
    {
        public OutputStreamAttribute(string contentType = null, 
            string filename = null, int bufferSize = 0)
        {
            ContentType = contentType;
            Filename = filename;
            if (bufferSize > 0) BufferSize = bufferSize;
        }

        public string Filename { get; }
        public string ContentType { get; }
        public int? BufferSize { get; }
    }

    public class OutputStream : IStreamInfo
    {
        public string Filename { get; set; }
        public string ContentType { get; set; }
        public Stream Stream { get; set; }
        public int? BufferSize { get; set; }
    }
    
    public class StreamWriter : IResponseWriter
    {
        private readonly Configuration _configuration;

        public StreamWriter(Configuration configuration)
        {
            _configuration = configuration;
        }

        public virtual bool AppliesTo(ResponseWriterContext context)
        {
            var responseType = context.RequestContext.Route.ResponseType?.Type;
            return context.RequestContext.Route.HasResponse && 
                (responseType == typeof(OutputStream) || responseType == typeof(Stream));
        }

        public Task<HttpResponseMessage> Write(ResponseWriterContext context)
        {
            var response = new HttpResponseMessage();
            var streamInfo = context.Response.As<IStreamInfo>() ?? context
                .RequestContext.Action.Method.GetAttribute<OutputStreamAttribute>();
            var stream = context.Response.As<OutputStream>()?.Stream ?? context.Response as Stream;
            var bufferSize = streamInfo?.BufferSize ?? _configuration.DownloadBufferSize;
            
            if (stream != null)
            {
                response.Content = new AsyncStreamContent(stream, bufferSize);
                if (streamInfo != null)
                {
                    if (streamInfo.ContentType.IsNotNullOrEmpty())
                        response.Content.Headers.SetContentType(streamInfo.ContentType);
                    if (streamInfo.Filename.IsNotNullOrEmpty())
                        response.Content.Headers.SetAttachmentDisposition(streamInfo.Filename);
                }
                if (response.Content.Headers.ContentType == null)
                    response.Content.Headers.SetContentType(MimeTypes.ApplicationOctetStream);

            }
            return response.ToTaskResult();
        }
    }
}