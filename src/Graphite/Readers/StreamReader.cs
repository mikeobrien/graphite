using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Routing;

namespace Graphite.Readers
{
    public class InputStream : InputBody<Stream> { }

    public class StreamReader : BodyReaderBase<Stream, InputStream>
    {
        public StreamReader(RouteDescriptor routeDescriptor, HttpRequestMessage requestMessage) : 
            base(routeDescriptor, requestMessage) { }

        protected override async Task<Stream> GetData(HttpContent content)
        {
            return await content.ReadAsStreamAsync();
        }
    }
}