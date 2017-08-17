using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Graphite.Extensions;

namespace Graphite.Http
{
    public class AsyncContent : HttpContent
    {
        private readonly Func<Stream, Task> _writeToStream;

        public AsyncContent(byte[] data)
        {
            _writeToStream = output => output.WriteAsync(data);
        }

        public AsyncContent(Stream stream, int bufferSize)
        {
            _writeToStream = output => stream.CopyToAsync(output, bufferSize);
        }

        public AsyncContent(string data, Encoding encoding)
        {
            _writeToStream = output => output.WriteAsync(data, encoding);
        }

        public AsyncContent(Func<Stream, Task> writeToStream)
        {
            _writeToStream = writeToStream;
        }

        protected override Task SerializeToStreamAsync(
            Stream stream, TransportContext context)
        {
            return _writeToStream(stream);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }
}