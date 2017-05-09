using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Routing;

namespace Graphite.Readers
{
    public class InputBytes : InputBody<byte[]> { }

    public class ByteReader : BodyReaderBase<byte[], InputBytes>
    {
        public ByteReader(RouteDescriptor routeDescriptor, HttpRequestMessage requestMessage) : 
            base(routeDescriptor, requestMessage) { }

        protected override async Task<byte[]> GetData(HttpContent content)
        {
            return await content.ReadAsByteArrayAsync();
        }
    }
}