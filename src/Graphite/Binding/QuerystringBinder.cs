using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graphite.Extensions;
using Graphite.Routing;

namespace Graphite.Binding
{
    public class QuerystringBinder : ParameterBinderBase
    {
        public QuerystringBinder(IEnumerable<IValueMapper> mappers) : base(mappers) { }

        public override bool AppliesTo(RequestBinderContext context)
        {
            return context.RequestContext.Route.Parameters.Any();
        }

        protected override ActionParameter[] GetParameters(RequestBinderContext context)
        {
            return context.RequestContext.Route.Parameters;
        }

        protected override Task<ILookup<string, object>> 
            GetValues(RequestBinderContext context)
        {
            return context.RequestContext.QuerystringParameters
                .ToTaskResult<ILookup<string, object>>();
        }
    }
}