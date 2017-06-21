using System.Net.Http;
using System.Text;
using Graphite.Extensions;
using Graphite.Http;
using Graphite.Routing;

namespace Graphite.Writers
{
    public class XmlWriter : StringWriterBase
    {
        private readonly RouteDescriptor _routeDescriptor;

        public XmlWriter(RouteDescriptor routeDescriptor, 
            HttpRequestMessage requestMessage, HttpResponseMessage responseMessage) : 
            base(requestMessage, responseMessage, Encoding.UTF8, MimeTypes.ApplicationXml)
        {
            _routeDescriptor = routeDescriptor;
        }

        protected override string GetResponse(ResponseWriterContext context)
        {
            return context.Response.SerializeXml(_routeDescriptor.ResponseType.Type);
        }
    }
}