using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Routing;

namespace Graphite.Readers
{
    public class InputString : InputBody<string> { }

    public class StringReader : BodyReaderBase<string, InputString>
    {
        public StringReader(RouteDescriptor routeDescriptor, HttpRequestMessage requestMessage) : 
            base(routeDescriptor, requestMessage) { }

        protected override async Task<string> GetData(HttpContent content)
        {
            return await content.ReadAsStringAsync();
        }
    }
}