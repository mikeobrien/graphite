using System.Net.Http;
using System.Text;
using Bender;
using Bender.Configuration;
using Graphite.Http;
using Graphite.Routing;
using Graphite.Writers;

namespace Graphite.Bender
{
    public class XmlWriter : StringWriterBase
    {
        private readonly Options _options;
        private readonly RouteDescriptor _routeDescriptor;

        public XmlWriter(Options options, RouteDescriptor routeDescriptor,
            HttpRequestMessage requestMessage, HttpResponseMessage responseMessage) : 
            base(requestMessage, responseMessage, MimeTypes.ApplicationXml, Encoding.UTF8)
        {
            _options = options;
            _routeDescriptor = routeDescriptor;
        }

        protected override string GetResponse(ResponseWriterContext context)
        {
            return context.Response.SerializeXml(
                _routeDescriptor.ResponseType.Type, _options);
        }
    }
}