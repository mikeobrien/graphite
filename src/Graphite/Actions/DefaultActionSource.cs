using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Graphite.Routing;

namespace Graphite.Actions
{
    public class DefaultActionSource : IActionSource
    {
        private readonly Configuration _configuration;
        private readonly HttpConfiguration _httpConfiguration;
        private readonly IEnumerable<IActionMethodSource> _actionMethodSources;
        private readonly IEnumerable<IRouteConvention> _routeConventions;
        private readonly ActionDescriptorFactory _actionDescriptorFactory;

        public DefaultActionSource(Configuration configuration,
            HttpConfiguration httpConfiguration,
            IEnumerable<IActionMethodSource> actionMethodSources,
            IEnumerable<IRouteConvention> routeConventions,
            ActionDescriptorFactory actionDescriptorFactory)
        {
            _configuration = configuration;
            _httpConfiguration = httpConfiguration;
            _actionMethodSources = actionMethodSources;
            _routeConventions = routeConventions;
            _actionDescriptorFactory = actionDescriptorFactory;
        }

        public virtual bool Applies()
        {
            return true;
        }

        public virtual List<ActionDescriptor> GetActions()
        {
            return _actionMethodSources
                .SelectMany(x => x.GetActionMethods()).Distinct()
                .SelectMany(a => _routeConventions
                    .ThatApplyTo(a, _configuration, _httpConfiguration)
                    .SelectMany(rc => rc
                        .GetRouteDescriptors(a)
                        .Select(r => _actionDescriptorFactory.CreateDescriptor(a, r))))
                .ToList();
        }
    }
}