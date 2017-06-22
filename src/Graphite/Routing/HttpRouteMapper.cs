using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Routing;
using Graphite.Actions;
using Graphite.DependencyInjection;

namespace Graphite.Routing
{
    public class HttpRouteConfiguration
    {
        public string Name { get; set; }
        public string RouteTemplate { get; set; }
        public HttpMessageHandler MessageHandler { get; set; }
        public Dictionary<string, object> Constraints { get; set; }
        public List<string> MethodConstraints { get; set; }
    }

    public class HttpRouteMapper : IHttpRouteMapper
    {
        private readonly HttpConfiguration _httpConfiguration;
        private readonly IContainer _container;
        private readonly IInlineConstraintResolver _constraintResolver;
        private readonly List<IHttpRouteDecorator> _routeDecorators;
        private readonly ConfigurationContext _configurationContext;

        public HttpRouteMapper(HttpConfiguration httpConfiguration,
            IContainer container, IInlineConstraintResolver constraintResolver,
            List<IHttpRouteDecorator> routeDecorators,
            ConfigurationContext configurationContext)
        {
            _httpConfiguration = httpConfiguration;
            _container = container;
            _constraintResolver = constraintResolver;
            _routeDecorators = routeDecorators;
            _configurationContext = configurationContext;
        }

        public void Map(ActionDescriptor actionDescriptor)
        {
            var routeConfiguration = new HttpRouteConfiguration
            {
                Name = actionDescriptor.Route.Id,
                RouteTemplate = actionDescriptor.Route.Url.RemoveConstraints(),
                MessageHandler = _container.GetInstance<ActionMessageHandler>(actionDescriptor),
                Constraints = actionDescriptor.Route.GetRouteConstraints(_constraintResolver),
                MethodConstraints = new List<string> { actionDescriptor.Route.Method }
            };

            _routeDecorators.ThatApplyTo(actionDescriptor, routeConfiguration,
                _configurationContext).Decorate(routeConfiguration);

            _httpConfiguration.Routes.MapHttpRoute(routeConfiguration.Name,
                routeConfiguration.RouteTemplate, null,
                handler: routeConfiguration.MessageHandler,
                constraints: routeConfiguration.Constraints
                    .AddMethodConstraints(routeConfiguration.MethodConstraints));
        }
    }
}