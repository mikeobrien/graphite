using System.Collections.Generic;
using System.Linq;
using Graphite.Routing;

namespace Graphite.Actions
{
    public class DefaultActionSource : IActionSource
    {
        private readonly ConfigurationContext _configurationContext;
        private readonly IEnumerable<IActionMethodSource> _actionMethodSources;
        private readonly IEnumerable<IRouteConvention> _routeConventions;
        private readonly ActionDescriptorFactory _actionDescriptorFactory;

        public DefaultActionSource(ConfigurationContext configurationContext,
            IEnumerable<IActionMethodSource> actionMethodSources,
            IEnumerable<IRouteConvention> routeConventions,
            ActionDescriptorFactory actionDescriptorFactory)
        {
            _actionMethodSources = actionMethodSources;
            _routeConventions = routeConventions;
            _actionDescriptorFactory = actionDescriptorFactory;
            _configurationContext = configurationContext;
        }

        public virtual bool Applies()
        {
            return true;
        }

        public virtual List<ActionDescriptor> GetActions()
        {
            return _actionMethodSources
                .SelectMany(x => x.GetActionMethods()).Distinct()
                .SelectMany(a => _routeConventions.ThatApplyTo(a, _configurationContext)
                    .SelectMany(rc => rc
                        .GetRouteDescriptors(a)
                        .Select(r => _actionDescriptorFactory.CreateDescriptor(a, r))))
                .ToList();
        }
    }
}