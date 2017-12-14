using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Graphite.Extensions;
using Graphite.Linq;
using Graphite.Routing;

namespace Graphite.Http
{
    public interface IUrlParameters : ILookup<string, object> { }

    public class UrlParameters : ParametersBase<object>, IUrlParameters
    {
        public UrlParameters(HttpRequestMessage request, RouteDescriptor routeDescriptor)
            : base(GetParameters(request, routeDescriptor)) { }

        private static IEnumerable<KeyValuePair<string, object>> GetParameters(
            HttpRequestMessage request, RouteDescriptor routeDescriptor)
        {
            var parameters = request.GetRequestContext().RouteData.Values;
            return routeDescriptor.UrlParameters.Any(x => x.IsWildcard)
                ? parameters.SelectMany(x => ExpandWildcardParameters(x, routeDescriptor))
                : parameters;
        }

        private static IEnumerable<KeyValuePair<string, object>> ExpandWildcardParameters
            (KeyValuePair<string, object> value, RouteDescriptor routeDescriptor)
        {
            var wildcard = routeDescriptor.UrlParameters.FirstOrDefault(x => 
                x.IsWildcard && x.Name.EqualsUncase(value.Key));
            return wildcard == null || (!wildcard.TypeDescriptor.IsArray && 
                    !wildcard.TypeDescriptor.IsGenericListCastable)
                ? new[] { value }
                : value.Value.Split('/').ToKeyValuePairs<string, object>(value.Key);
        }
    }
}