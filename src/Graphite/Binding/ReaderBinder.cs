using System.Collections.Generic;
using System.Threading.Tasks;
using Graphite.Actions;
using Graphite.Readers;
using Graphite.Routing;

namespace Graphite.Binding
{
    public class ReaderBinder : IRequestBinder
    {
        private readonly IEnumerable<IRequestReader> _readers;
        private readonly Configuration _configuration;
        private readonly RouteDescriptor _routeDescriptor;
        private readonly ActionConfigurationContext _actionConfigurationContext;

        public ReaderBinder(
            ActionConfigurationContext actionConfigurationContext, 
            RouteDescriptor routeDescriptor,
            IEnumerable<IRequestReader> readers)
        {
            _actionConfigurationContext = actionConfigurationContext;
            _configuration = actionConfigurationContext.Configuration;
            _routeDescriptor = routeDescriptor;
            _readers = readers;
        }

        public virtual bool AppliesTo(RequestBinderContext context)
        {
            return _routeDescriptor.HasRequest;
        }

        public virtual async Task Bind(RequestBinderContext context)
        {
            var position = _routeDescriptor.RequestParameter.Position;
            var reader = _readers.ThatApply(_actionConfigurationContext);
            if (reader != null) context.ActionArguments[position] = await reader.Read(context);
        }
    }
}