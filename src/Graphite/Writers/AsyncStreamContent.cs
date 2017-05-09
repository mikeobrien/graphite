using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Graphite.Writers
{
    public class AsyncStreamContent : HttpContent
    {
        private readonly Stream _stream;
        private readonly int _bufferSize;

        public AsyncStreamContent(Stream stream, int bufferSize)
        {
            _stream = stream;
            _bufferSize = bufferSize;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            return _stream.CopyToAsync(stream, _bufferSize);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }
}