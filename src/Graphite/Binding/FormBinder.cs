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
        private readonly ParameterBinder _parameterBinder;
        private readonly HttpRequestMessage _requestMessage;

        public FormBinder(
            RouteDescriptor routeDescriptor,
            ParameterBinder parameterBinder,
            HttpRequestMessage requestMessage)
        {
            _parameterBinder = parameterBinder;
            _routeDescriptor = routeDescriptor;
            _requestMessage = requestMessage;
        }

        public bool AppliesTo(RequestBinderContext context)
        {
            return !_routeDescriptor.HasRequest && _routeDescriptor.Parameters.Any() &&
                _requestMessage.ContentTypeIs(MimeTypes.ApplicationFormUrlEncoded);
        }

        public async Task Bind(RequestBinderContext context)
        {
            var data = await _requestMessage.Content.ReadAsStringAsync();
            _parameterBinder.Bind(data.ParseQueryString(), context.ActionArguments, 
                _routeDescriptor.Parameters);
        }
    }
}