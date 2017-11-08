using System.IO;
using System.Net.Http.Headers;
using Graphite.Readers;

namespace Graphite.Binding
{
    public class InputBytes : InputBody<byte[]>
    {
        public InputBytes(string name, string filename, 
            string contentType, string[] contentEncoding, 
            long? contentLength, HttpHeaders headers, byte[] data) : 
            base(name, filename, contentType, contentEncoding, 
                contentLength, headers, data) { }
    }

    public class InputString : InputBody<string>
    {
        public InputString(string name, string filename, 
            string contentType, string[] contentEncoding, 
            long? contentLength, HttpHeaders headers, string data) : 
            base(name, filename, contentType, contentEncoding, 
                contentLength, headers, data) { }
    }

    public class InputStream : InputBody<Stream>
    {
        public InputStream(string name, string filename, 
            string contentType, string[] contentEncoding, 
            long? contentLength, HttpHeaders headers, Stream data) : 
            base(name, filename, contentType, contentEncoding, 
                contentLength, headers, data) { }
    }

    public abstract class InputBody<T>
    {
        protected InputBody(ReaderContext context, T data)
            :this(context.Name, context.Filename, context.ContentType, 
                 context.ContentEncoding, context.ContentLength,
                 context.Headers, data) { }

        protected InputBody(string name, string filename, string contentType, 
            string[] contentEncoding, long? contentLength, HttpHeaders headers, T data)
        {
            Name = name;
            Filename = filename;
            ContentType = contentType;
            Encoding = contentEncoding;
            Length = contentLength;
            Headers = headers;
            Data = data;
        }

        public string Name { get; }
        public string Filename { get; }
        public string ContentType { get; }
        public string[] Encoding { get; }
        public HttpHeaders Headers { get; }
        public long? Length { get; }
        public T Data { get; }
    }
}