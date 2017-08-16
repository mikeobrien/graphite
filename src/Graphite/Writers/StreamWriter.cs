using System;
using System.IO;
using System.Net.Http;
using Graphite.Actions;
using Graphite.Http;
using Graphite.Routing;

namespace Graphite.Writers
{
    public interface IStreamOutputInfo : IOutputInfo
    {
        int? BufferSize { get; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class OutputStreamAttribute : OutputInfoAttribute, IStreamOutputInfo
    {
        public OutputStreamAttribute(string contentType = null,
            string filename = null, int bufferSize = 0) :
            base(contentType, filename)
        {
            if (bufferSize > 0) BufferSize = bufferSize;
        }

        public int? BufferSize { get; }
    }

    public class OutputStream : OutputBody<Stream>, IStreamOutputInfo
    {
        public int? BufferSize { get; set; }
    }

    public class StreamWriter : BodyWriterBase<Stream,
        OutputStream, OutputStreamAttribute, IStreamOutputInfo>
    {
        private readonly Configuration _configuration;

        public StreamWriter(Configuration configuration,
            ActionMethod actionMethod, 
            RouteDescriptor routeDescriptor, 
            HttpResponseMessage responseMessage) : 
            base(actionMethod, routeDescriptor, 
                responseMessage, configuration)
        {
            _configuration = configuration;
        }

        protected override HttpContent GetContent(Stream data, IStreamOutputInfo outputInfo)
        {
            return new AsyncStreamContent(data, outputInfo?.BufferSize ?? 
                _configuration.DownloadBufferSize);
        }

        protected override string GetContentType(Stream data)
        {
            return MimeTypes.ApplicationOctetStream;
        }
    }
}