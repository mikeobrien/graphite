using System.Collections.Generic;
using System.Linq;
using Graphite.Behaviors;
using Graphite.Reflection;
using Graphite.Routing;

namespace Graphite.Actions
{
    public class DefaultActionSource : IActionSource
    {
        private readonly ConfigurationContext _configurationContext;
        private readonly IEnumerable<IActionMethodSource> _actionMethodSources;
        private readonly IEnumerable<IRouteConvention> _routeConventions;
        private readonly ITypeCache _typeCache;

        public DefaultActionSource(ConfigurationContext configurationContext,
            IEnumerable<IActionMethodSource> actionMethodSources,
            IEnumerable<IRouteConvention> routeConventions, 
            ITypeCache typeCache)
        {
            _actionMethodSources = actionMethodSources;
            _routeConventions = routeConventions;
            _configurationContext = configurationContext;
            _typeCache = typeCache;
        }

        public virtual bool Applies()
        {
            return true;
        }

        public virtual List<ActionDescriptor> GetActions()
        {
            return _actionMethodSources.ThatApplyTo(_configurationContext)
                .SelectMany(x => x.GetActionMethods()).Distinct()
                .SelectMany(a => _routeConventions.ThatApplyTo(a, _configurationContext)
                    .SelectMany(rc => rc
                        .GetRouteDescriptors(a)
                        .Select(r => new ActionDescriptor(a, r,
                            _configurationContext.Configuration.Behaviors
                                .ThatApplyTo(a, r, _configurationContext)
                                .Select(x => _typeCache.GetTypeDescriptor(x)).ToArray()))))
                .ToList();
        }
    }
}