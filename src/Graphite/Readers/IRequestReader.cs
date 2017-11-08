using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Graphite.Extensibility;
using Graphite.Reflection;

namespace Graphite.Readers
{
    public class ReaderContext
    {
        public ReaderContext(TypeDescriptor readType, string contentType, 
            string[] contentEncoding, string filename, HttpHeaders headers, 
            Task<Stream> data, string name = null, long? contentLength = null)
        {
            ContentType = contentType;
            Data = data;
            Name = name;
            Headers = headers;
            ReadType = readType;
            Filename = filename;
            ContentLength = contentLength;
            ContentEncoding = contentEncoding;
        }

        public string Name { get; }
        public TypeDescriptor ReadType { get; }
        public string ContentType { get; }
        public string[] ContentEncoding { get; }
        public string Filename { get; }
        public long? ContentLength { get; }
        public HttpHeaders Headers { get; }
        public Task<Stream> Data { get; }
    }

    public interface IRequestReader : IConditional<ReaderContext>
    {
        Task<ReadResult> Read(ReaderContext context);
    }
}