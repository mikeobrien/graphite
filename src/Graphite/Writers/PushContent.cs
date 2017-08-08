using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Graphite.Writers
{
    public class PushContent : HttpContent
    {
        private readonly Func<Stream, TransportContext, Task> _writeToString;

        public PushContent(Func<Stream, TransportContext, Task> writeToString)
        {
            _writeToString = writeToString;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            return _writeToString(stream, context);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }
}