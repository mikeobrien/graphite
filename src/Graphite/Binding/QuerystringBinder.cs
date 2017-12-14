using System.Linq;
using System.Threading.Tasks;
using Graphite.Extensions;
using Graphite.Http;
using Graphite.Routing;

namespace Graphite.Binding
{
    public class QuerystringBinder : IRequestBinder
    {
        private readonly RouteDescriptor _routeDescriptor;
        private readonly ArgumentBinder _argumentBinder;
        private readonly IQuerystringParameters _querystringParameters;

        public QuerystringBinder(RouteDescriptor routeDescriptor,
            ArgumentBinder argumentBinder,
            IQuerystringParameters querystringParameters)
        {
            _argumentBinder = argumentBinder;
            _routeDescriptor = routeDescriptor;
            _querystringParameters = querystringParameters;
        }

        public bool AppliesTo(RequestBinderContext context)
        {
            return _routeDescriptor.Parameters.Any();
        }

        public Task<BindResult> Bind(RequestBinderContext context)
        {
            return _argumentBinder.Bind(_querystringParameters, 
                context.ActionArguments, _routeDescriptor.Parameters)
                .ToTaskResult();
        }
    }
}