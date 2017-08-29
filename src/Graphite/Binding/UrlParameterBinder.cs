using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graphite.Extensions;
using Graphite.Http;
using Graphite.Linq;
using Graphite.Routing;

namespace Graphite.Binding
{
    public class UrlParameterBinder : IRequestBinder
    {
        private readonly ArgumentBinder _argumentBinder;
        private readonly RouteDescriptor _routeDescriptor;
        private readonly UrlParameters _urlParameters;

        public UrlParameterBinder(
            RouteDescriptor routeDescriptor,
            ArgumentBinder argumentBinder,
            UrlParameters urlParameters)
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
            var values = _urlParameters.SelectMany(x =>
                ExpandWildcardParameters(x, _routeDescriptor
                    .UrlParameters)).ToLookup();
            var parameters = _routeDescriptor.UrlParameters
                .Concat(_routeDescriptor.Parameters).ToArray();
            return _argumentBinder.Bind(values, context.ActionArguments, 
                parameters).ToTaskResult();
        }

        private IEnumerable<KeyValuePair<string, object>> ExpandWildcardParameters
            (KeyValuePair<string, object> value, IEnumerable<UrlParameter> parameters)
        {
            var wildcard = parameters.FirstOrDefault(x => 
                x.IsWildcard && x.Name.EqualsIgnoreCase(value.Key));
            return wildcard == null || !(wildcard.TypeDescriptor.IsArray ||
                wildcard.TypeDescriptor.IsGenericListCastable)
                    ? new[] { value }
                    : value.Value.Split('/').ToKeyValuePairs(value.Key);
        }
    }
}