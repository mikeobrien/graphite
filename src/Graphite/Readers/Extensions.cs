using System.IO;
using System.Linq;
using System.Net.Http;
using Graphite.Actions;
using Graphite.Binding;
using Graphite.Http;

namespace Graphite.Readers
{
    public static class Extensions
    {
        public static ReaderContext CreateReaderContext(this HttpContent content,
            ActionDescriptor actionDescriptor)
        {
            var headers = content?.Headers;
            return new ReaderContext(
                actionDescriptor.Route.RequestParameter?.ParameterType,
                headers?.ContentType?.MediaType,
                headers?.ContentEncoding?.ToArray(),
                headers?.ContentDisposition?.FileName.Unquote(),
                headers,
                content?.ReadAsStreamAsync(),
                contentLength: headers?.ContentLength);
        }

        public static InputStream CreateInputStream(this ReaderContext context, Stream data)
        {
            return new InputStream(
                context.Name,
                context.Filename,
                context.ContentType,
                context.ContentEncoding,
                context.ContentLength,
                context.Headers,
                data);
        }

        public static InputString CreateInputString(this ReaderContext context, string data)
        {
            return new InputString(
                context.Name,
                context.Filename,
                context.ContentType,
                context.ContentEncoding,
                context.ContentLength,
                context.Headers,
                data);
        }

        public static InputBytes CreateInputBytes(this ReaderContext context, byte[] data)
        {
            return new InputBytes(
                context.Name,
                context.Filename,
                context.ContentType,
                context.ContentEncoding,
                context.ContentLength,
                context.Headers,
                data);
        }
    }
}
