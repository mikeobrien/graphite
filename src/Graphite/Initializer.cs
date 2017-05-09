using System;
using System.Collections.Generic;
using System.Linq;
using Graphite.Actions;
using Graphite.DependencyInjection;
using Graphite.Routing;

namespace Graphite
{
    public class Initializer : IInitializer
    {
        private readonly IRouteMapper _routeMapper;
        private readonly IEnumerable<IActionSource> _actionSources;
        private readonly IContainer _container;
        private readonly IEnumerable<IActionDecorator> _actionDecorators;
        private readonly ConfigurationContext _configurationContext;

        public Initializer(IRouteMapper routeMapper, 
            IEnumerable<IActionSource> actionSources, IContainer container,
            IEnumerable<IActionDecorator> actionDecorators,
            ConfigurationContext configurationContext)
        {
            _routeMapper = routeMapper;
            _actionSources = actionSources;
            _container = container;
            _actionDecorators = actionDecorators;
            _configurationContext = configurationContext;
        }

        public void Initialize()
        {
            var actions = _actionSources.ThatApplyTo(_configurationContext)
                .SelectMany(x => x.GetActions())
                .OrderBy(x => x.Route.Url, StringComparer.OrdinalIgnoreCase).ToList();

            var duplicates = actions.GroupBy(x => x.Route.Id)
                .Where(x => x.Count() > 1).ToList();

            if (duplicates.Any()) throw new DuplicateRouteException(duplicates);

            actions.ForEach(a => _actionDecorators.ThatApplyTo(a, 
                _configurationContext).Decorate(a));
            
            _container.Register(new RuntimeConfiguration(actions));

            actions.ForEach(x => _routeMapper.Map(x));
        }
    }
}
