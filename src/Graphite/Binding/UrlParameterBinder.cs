using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graphite.Extensions;
using Graphite.Routing;

namespace Graphite.Binding
{
    public class UrlParameterBinder : ParameterBinderBase
    {
        public UrlParameterBinder(IEnumerable<IValueMapper> mappers) : base(mappers) { }

        public override bool AppliesTo(RequestBinderContext context)
        {
            return context.RequestContext.Route.UrlParameters.Any();
        }

        protected override Task<ILookup<string, object>> GetValues(RequestBinderContext context)
        {
            return context.RequestContext.UrlParameters.SelectMany(x => 
                ExpandWildcardParameters(x, context.RequestContext.Route
                    .UrlParameters)).ToLookup().ToTaskResult();
        }

        private IEnumerable<KeyValuePair<string, object>> ExpandWildcardParameters
            (KeyValuePair<string, object> value, UrlParameter[] parameters)
        {
            var wildcard = parameters.FirstOrDefault(x => x.IsWildcard &&
                x.Name.EqualsIgnoreCase(value.Key));
            return wildcard == null || !(wildcard.TypeDescriptor.IsArray ||
                    wildcard.TypeDescriptor.IsGenericListCastable)
                ? new[] { value }
                : value.Value.Split('/').ToKeyValuePairs(value.Key);
        }

        protected override ActionParameter[] GetParameters(RequestBinderContext context)
        {
            return context.RequestContext.Route.UrlParameters
                .Concat(context.RequestContext.Route.Parameters).ToArray();
        }
    }
}