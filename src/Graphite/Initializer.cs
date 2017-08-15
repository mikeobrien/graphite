using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Graphite.Actions;
using Graphite.DependencyInjection;
using Graphite.Routing;

namespace Graphite
{
    public interface IInitializer
    {
        void Initialize();
    }

    public class Initializer : IInitializer
    {
        private readonly IHttpRouteMapper _routeMapper;
        private readonly IEnumerable<IActionSource> _actionSources;
        private readonly IContainer _container;
        private readonly IEnumerable<IActionDecorator> _actionDecorators;
        private readonly Configuration _configuration;
        private readonly HttpConfiguration _httpConfiguration;

        public Initializer(IHttpRouteMapper routeMapper, 
            IEnumerable<IActionSource> actionSources, IContainer container,
            IEnumerable<IActionDecorator> actionDecorators, 
            Configuration configuration, HttpConfiguration httpConfiguration)
        {
            _routeMapper = routeMapper;
            _actionSources = actionSources;
            _container = container;
            _actionDecorators = actionDecorators;
            _configuration = configuration;
            _httpConfiguration = httpConfiguration;
        }

        public void Initialize()
        {
            var actions = _actionSources
                .SelectMany(x => x.GetActions())
                .OrderBy(x => x.Route.Url, StringComparer.OrdinalIgnoreCase).ToList();

            var duplicates = actions.GroupBy(x => x.Route.Id)
                .Where(x => x.Count() > 1).ToList();

            if (duplicates.Any()) throw new DuplicateRouteException(duplicates);

            actions.ForEach(a => _actionDecorators.ThatApplyTo(a, 
                _configuration, _httpConfiguration).Decorate(a));
            
            _container.Register(new RuntimeConfiguration(actions));

            actions.ForEach(x => _routeMapper.Map(x));
        }
    }
}
