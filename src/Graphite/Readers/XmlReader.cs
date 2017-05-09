using System.Net.Http;
using System.Text;
using Graphite.Extensions;
using Graphite.Http;
using Graphite.Routing;

namespace Graphite.Readers
{
    public class XmlReader : StringReaderBase
    {
        private readonly RouteDescriptor _routeDescriptor;

        public XmlReader(RouteDescriptor routeDescriptor, HttpRequestMessage requestMessage) : 
            base(requestMessage, MimeTypes.ApplicationXml)
        {
            _routeDescriptor = routeDescriptor;
        }

        protected override object GetRequest(string data)
        {
            return data.DeserializeXml(_routeDescriptor
                .RequestParameter.ParameterType.Type, Encoding.UTF8);
        }
    }
}