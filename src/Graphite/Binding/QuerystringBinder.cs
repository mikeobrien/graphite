using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graphite.Extensions;
using Graphite.Reflection;

namespace Graphite.Binding
{
    public class QuerystringBinder : ParameterBinderBase
    {
        public QuerystringBinder(IEnumerable<IValueMapper> mappers,
            Configuration configuration) : base(mappers, configuration) { }

        public override bool AppliesTo(RequestBinderContext context)
        {
            return context.RequestContext.Route.QuerystringParameters.Any();
        }

        protected override ParameterDescriptor[] GetParameters(RequestBinderContext context)
        {
            return context.RequestContext.Route.QuerystringParameters;
        }

        protected override Task<ILookup<string, string>> 
            GetValues(RequestBinderContext context)
        {
            return context.RequestContext.QuerystringParameters.ToTaskResult();
        }
    }
}