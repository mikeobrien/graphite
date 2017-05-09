using System.Net.Http;
using Bender;
using Bender.Configuration;
using Graphite.Http;
using Graphite.Readers;
using Graphite.Routing;

namespace Graphite.Bender
{
    public class XmlReader : StringReaderBase
    {
        private readonly Options _options;
        private readonly RouteDescriptor _routeDescriptor;

        public XmlReader(Options options, RouteDescriptor routeDescriptor,
            HttpRequestMessage requestMessage) :
            base(requestMessage, MimeTypes.ApplicationXml)
        {
            _options = options;
            _routeDescriptor = routeDescriptor;
        }

        protected override object GetRequest(string data)
        {
            return data.DeserializeXml(_routeDescriptor
                .RequestParameter.ParameterType.Type, _options);
        }
    }
}