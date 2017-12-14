using System.Linq;
using System.Threading.Tasks;
using Graphite.Extensions;
using Graphite.Http;
using Graphite.Routing;

namespace Graphite.Binding
{
    public class UrlParameterBinder : IRequestBinder
    {
        private readonly ArgumentBinder _argumentBinder;
        private readonly RouteDescriptor _routeDescriptor;
        private readonly IUrlParameters _urlParameters;

        public UrlParameterBinder(
            RouteDescriptor routeDescriptor,
            ArgumentBinder argumentBinder,
            IUrlParameters urlParameters)
        {
            _argumentBinder = argumentBinder;
            _routeDescriptor = routeDescriptor;
            _urlParameters = urlParameters;
        }

        public bool AppliesTo(RequestBinderContext context)
        {
            return _routeDescriptor.UrlParameters.Any();
        }

        public Task<BindResult> Bind(RequestBinderContext context)
        {
            var parameters = _routeDescriptor.UrlParameters
                .Concat(_routeDescriptor.Parameters).ToArray();
            return _argumentBinder.Bind(_urlParameters, context
                .ActionArguments, parameters).ToTaskResult();
        }
    }
}