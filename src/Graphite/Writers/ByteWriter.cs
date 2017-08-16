using System;
using System.Net.Http;
using Graphite.Actions;
using Graphite.Http;
using Graphite.Routing;

namespace Graphite.Writers
{
    [AttributeUsage(AttributeTargets.Method)]
    public class OutputBytesAttribute : OutputInfoAttribute
    {
        public OutputBytesAttribute(string contentType = null,
            string filename = null) : base(contentType, filename) { }
    }

    public class OutputBytes : OutputBody<byte[]> { }

    public class ByteWriter : BodyWriterBase<byte[], 
        OutputBytes, OutputBytesAttribute, IOutputInfo>
    {
        public ByteWriter(ActionMethod actionMethod, 
            RouteDescriptor routeDescriptor,
            HttpResponseMessage responseMessage,
            Configuration configuration) : 
            base(actionMethod, routeDescriptor, 
                responseMessage, configuration) { }

        protected override HttpContent GetContent(byte[] data, IOutputInfo outputInfo)
        {
            return new AsyncByteContent(data);
        }

        protected override string GetContentType(byte[] data)
        {
            return MimeTypes.ApplicationOctetStream;
        }
    }
}