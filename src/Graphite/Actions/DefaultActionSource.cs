using System.Collections.Generic;
using System.Linq;
using Graphite.Reflection;
using Graphite.Routing;

namespace Graphite.Actions
{
    public class DefaultActionSource : IActionSource
    {
        private readonly IEnumerable<IActionMethodSource> _actionMethodSources;
        private readonly IEnumerable<IRouteConvention> _routeConventions;
        private readonly Configuration _configuration;
        private readonly ITypeCache _typeCache;

        public DefaultActionSource(IEnumerable<IActionMethodSource> actionMethodSources,
            IEnumerable<IRouteConvention> routeConventions, Configuration configuration,
            ITypeCache typeCache)
        {
            _actionMethodSources = actionMethodSources;
            _routeConventions = routeConventions;
            _configuration = configuration;
            _typeCache = typeCache;
        }

        public virtual bool AppliesTo(ActionSourceContext context)
        {
            return true;
        }

        public virtual List<ActionDescriptor> GetActions(ActionSourceContext context)
        {
            return _actionMethodSources.ThatApplyTo(context, _configuration)
                .SelectMany(x => x.GetActionMethods(context)).Distinct()
                .SelectMany(a => _routeConventions.ThatApplyTo(a, context, _configuration)
                    .SelectMany(rc => rc
                        .GetRouteDescriptors(context, a)
                        .Select(r => new ActionDescriptor(a, r,
                                _configuration.Behaviors.ThatApplyTo(context, a, r)
                            .Select(x => _typeCache.GetTypeDescriptor(x)).ToArray()))))
                .ToList();
        }
    }
}