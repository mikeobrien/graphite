using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Extensions;

namespace Graphite.Writers
{
    public class AsyncByteContent : HttpContent
    {
        private readonly byte[] _data;

        public AsyncByteContent(byte[] data)
        {
            _data = data;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            return stream.WriteAsync(_data);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }
}