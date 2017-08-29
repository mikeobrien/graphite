using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Extensions;
using Graphite.Http;
using Graphite.Linq;
using Graphite.Routing;
using Newtonsoft.Json;

namespace Graphite.Binding
{
    public class JsonBinder : IRequestBinder
    {
        private readonly ArgumentBinder _argumentBinder;
        private readonly RouteDescriptor _routeDescriptor;
        private readonly HttpRequestMessage _requestMessage;

        public JsonBinder(RouteDescriptor routeDescriptor,
            ArgumentBinder argumentBinder,
            HttpRequestMessage requestMessage)
        {
            _argumentBinder = argumentBinder;
            _routeDescriptor = routeDescriptor;
            _requestMessage = requestMessage;
        }

        public bool AppliesTo(RequestBinderContext context)
        {
            return !_routeDescriptor.HasRequest && _routeDescriptor.Parameters.Any() &&
                   _requestMessage.ContentTypeIs(MimeTypes.ApplicationJson);
        }

        public async Task<BindResult> Bind(RequestBinderContext context)
        {
            var data = await _requestMessage.Content.ReadAsStringAsync();
            var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(data).ToLookup();
            return _argumentBinder.Bind(values, context.ActionArguments, _routeDescriptor.Parameters);
        }
    }
}