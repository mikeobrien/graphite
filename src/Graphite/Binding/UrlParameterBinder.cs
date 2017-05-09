using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graphite.Extensions;
using Graphite.Reflection;

namespace Graphite.Binding
{
    public class UrlParameterBinder : ParameterBinderBase
    {
        public UrlParameterBinder(IEnumerable<IValueMapper> mappers,
            Configuration configuration) : base(mappers, configuration) { }

        public override bool AppliesTo(RequestBinderContext context)
        {
            return context.RequestContext.Route.UrlParameters.Any();
        }

        protected override ParameterDescriptor[] GetParameters(RequestBinderContext context)
        {
            return context.RequestContext.Route.UrlParameters;
        }

        protected override Task<ILookup<string, string>> GetValues(RequestBinderContext context)
        {
            return context.RequestContext.UrlParameters.SelectMany(x => 
                ExpandWildcardParameters(x, context.RequestContext.Route
                    .WildcardParameters)).ToLookup().ToTaskResult();
        }

        private IEnumerable<KeyValuePair<string, string>> ExpandWildcardParameters
            (KeyValuePair<string, string> value, ParameterDescriptor[] wildcardParameters)
        {
            var wildcard = wildcardParameters.FirstOrDefault(x => x.Name.EqualsIgnoreCase(value.Key));
            return wildcard == null || !(wildcard.ParameterType.IsArray || 
                    wildcard.ParameterType.IsGenericListCastable) 
                ? new[] { value } 
                : value.Value.Split('/').Select(x => new KeyValuePair<string, string>(value.Key, x));
        }
    }
}