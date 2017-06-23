using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graphite.Extensions;
using Graphite.Http;
using Graphite.Routing;

namespace Graphite.Binding
{
    public class UrlParameterBinder : IRequestBinder
    {
        private readonly ParameterBinder _parameterBinder;
        private readonly RouteDescriptor _routeDescriptor;
        private readonly UrlParameters _urlParameters;

        public UrlParameterBinder(
            RouteDescriptor routeDescriptor,
            ParameterBinder parameterBinder,
            UrlParameters urlParameters)
        {
            _parameterBinder = parameterBinder;
            _routeDescriptor = routeDescriptor;
            _urlParameters = urlParameters;
        }

        public bool AppliesTo(RequestBinderContext context)
        {
            return _routeDescriptor.UrlParameters.Any();
        }

        public Task Bind(RequestBinderContext context)
        {
            var values = _urlParameters.SelectMany(x =>
                ExpandWildcardParameters(x, _routeDescriptor
                    .UrlParameters)).ToLookup();
            var parameters = _routeDescriptor.UrlParameters
                .Concat(_routeDescriptor.Parameters).ToArray();
            _parameterBinder.Bind(values, context.ActionArguments, parameters);
            return Task.CompletedTask;
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