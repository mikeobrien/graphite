using System.IO;
using System.Threading.Tasks;

namespace Graphite.Readers
{
    public class InputStream
    {
        public string Filename { get; set; }
        public string MimeType { get; set; }
        public long? Length { get; set; }
        public Stream Stream { get; set; }
    }

    public class StreamReader : IRequestReader
    {
        public virtual bool AppliesTo(RequestReaderContext context)
        {
            var requestType = context.RequestContext.Route.RequestParameter?.ParameterType.Type;
            return context.RequestContext.Route.HasRequest && 
                (requestType == typeof(InputStream) || requestType == typeof(Stream));
        }

        public async Task<object> Read(RequestReaderContext context)
        {
            var requestType = context.RequestContext.Route.RequestParameter?.ParameterType.Type;
            var stream = await context.RequestContext.RequestMessage.Content.ReadAsStreamAsync();
            if (requestType == typeof(Stream)) return stream;
            var headers = context.RequestContext.RequestMessage.Content.Headers;
            return new InputStream
            {
                Filename = headers.ContentDisposition?.FileName.Trim('"'),
                MimeType = headers.ContentType?.MediaType,
                Length = headers.ContentLength,
                Stream = stream
            };
        }
    }
}