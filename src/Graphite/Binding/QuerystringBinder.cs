using System.Linq;
using System.Threading.Tasks;
using Graphite.Http;
using Graphite.Routing;

namespace Graphite.Binding
{
    public class QuerystringBinder : IRequestBinder
    {
        private readonly RouteDescriptor _routeDescriptor;
        private readonly ParameterBinder _parameterBinder;
        private readonly QuerystringParameters _querystringParameters;

        public QuerystringBinder(RouteDescriptor routeDescriptor,
            ParameterBinder parameterBinder,
            QuerystringParameters querystringParameters)
        {
            _parameterBinder = parameterBinder;
            _routeDescriptor = routeDescriptor;
            _querystringParameters = querystringParameters;
        }

        public bool AppliesTo(RequestBinderContext context)
        {
            return _routeDescriptor.Parameters.Any();
        }

        public Task Bind(RequestBinderContext context)
        {
            _parameterBinder.Bind(_querystringParameters, 
                context.ActionArguments, _routeDescriptor.Parameters);
            return Task.CompletedTask;
        }
    }
}