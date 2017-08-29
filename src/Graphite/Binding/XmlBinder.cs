using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Graphite.Extensions;
using Graphite.Http;
using Graphite.Routing;

namespace Graphite.Binding
{
    public class XmlBinder : IRequestBinder
    {
        private readonly ArgumentBinder _argumentBinder;
        private readonly RouteDescriptor _routeDescriptor;
        private readonly HttpRequestMessage _requestMessage;

        public XmlBinder(
            RouteDescriptor routeDescriptor, 
            ArgumentBinder argumentBinder,
            HttpRequestMessage requestMessage)
        {
            _argumentBinder = argumentBinder;
            _routeDescriptor = routeDescriptor;
            _requestMessage = requestMessage;
        }

        public bool AppliesTo(RequestBinderContext context)
        {
            return !_routeDescriptor.HasRequest &&
                   _routeDescriptor.Parameters.Any() &&
                   _requestMessage.ContentTypeIs(MimeTypes.ApplicationXml);
        }

        public async Task<BindResult> Bind(RequestBinderContext context)
        {
            var values = await GetValues();
            return _argumentBinder.Bind(values, context.ActionArguments, _routeDescriptor.Parameters);
        }

        private async Task<ILookup<string, object>> GetValues()
        {
            var data = await _requestMessage.Content.ReadAsStringAsync();
            return data.IsNullOrEmpty() ? null :
                XDocument.Parse(data).Root?.Descendants().Where(x => !x.HasElements)
                    .ToLookup<XElement, string, object>(x => x.Name.ToString(), x => x.Value);
        }
    }
}