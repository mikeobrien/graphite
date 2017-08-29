using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Extensions;
using Graphite.Http;
using Graphite.Routing;

namespace Graphite.Binding
{
    public class FormBinder : IRequestBinder
    {
        private readonly RouteDescriptor _routeDescriptor;
        private readonly ArgumentBinder _argumentBinder;
        private readonly HttpRequestMessage _requestMessage;

        public FormBinder(
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
            return !_routeDescriptor.HasRequest && _routeDescriptor.Parameters.Any() &&
                _requestMessage.ContentTypeIs(MimeTypes.ApplicationFormUrlEncoded);
        }

        public async Task<BindResult> Bind(RequestBinderContext context)
        {
            var data = await _requestMessage.Content.ReadAsStringAsync();
            return _argumentBinder.Bind(data.ParseQueryString(), context.ActionArguments, 
                _routeDescriptor.Parameters);
        }
    }
}