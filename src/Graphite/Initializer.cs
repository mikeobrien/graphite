using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Routing;
using Graphite.Actions;
using Graphite.Behaviors;
using Graphite.DependencyInjection;
using Graphite.Extensions;
using Graphite.Monitoring;
using Graphite.Routing;

namespace Graphite
{
    public class Initializer : IInitializer
    {
        private readonly IEnumerable<IActionSource> _actionSources;
        private readonly IBehaviorChainInvoker _behaviorChainInvoker;
        private readonly IContainer _container;
        private readonly Configuration _configuration;
        private readonly Metrics _metrics;
        private readonly IEnumerable<IActionDecorator> _actionDecorators;

        public Initializer(IEnumerable<IActionSource> actionSources, 
            IBehaviorChainInvoker behaviorChainInvoker, IContainer container,
            Configuration configuration, Metrics metrics,
            IEnumerable<IActionDecorator> actionDecorators)
        {
            _actionSources = actionSources;
            _behaviorChainInvoker = behaviorChainInvoker;
            _container = container;
            _configuration = configuration;
            _metrics = metrics;
            _actionDecorators = actionDecorators;
        }

        public void Initialize(HttpConfiguration httpConfiguration)
        {
            var actions = _actionSources
                .ThatApplyTo(httpConfiguration, _configuration)
                .SelectMany(x => x.GetActions(_configuration, httpConfiguration)).ToList();

            var duplicates = actions.GroupBy(x => x.Route.Id)
                .Where(x => x.Count() > 1).ToList();

            if (duplicates.Any()) throw new DuplicateRouteException(duplicates);

            actions.ForEach(a => _actionDecorators
                .ThatApplyTo(a, httpConfiguration, _configuration)
                .ForEach(d => d.Decorate(a, httpConfiguration, _configuration)));

            _container.Register(httpConfiguration);
            _container.Register(new RuntimeConfiguration(actions));

            actions.ForEach(x =>
            {
                httpConfiguration.Routes.MapHttpRoute(x.Route.Id, x.Route.Url, null,
                    handler: new ActionMessageHandler(x, _behaviorChainInvoker,
                        _metrics, _configuration),
                    constraints: new
                    {
                        httpMethod = new HttpMethodConstraint(
                            new HttpMethod(x.Route.Method))
                    });
            });
        }
    }
}
