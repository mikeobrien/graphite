using System.Web.Http;
using System.Web.Http.Routing;
using Graphite.Actions;
using Graphite.DependencyInjection;

namespace Graphite.Routing
{
    public class RouteMapper : IRouteMapper
    {
        private readonly HttpConfiguration _httpConfiguration;
        private readonly IContainer _container;
        private readonly IInlineConstraintResolver _constraintResolver;

        public RouteMapper(HttpConfiguration httpConfiguration,
            IContainer container, IInlineConstraintResolver constraintResolver)
        {
            _httpConfiguration = httpConfiguration;
            _container = container;
            _constraintResolver = constraintResolver;
        }

        public void Map(ActionDescriptor actionDescriptor)
        {
            _httpConfiguration.Routes.MapHttpRoute(actionDescriptor.Route.Id, 
                actionDescriptor.Route.Url.RemoveConstraints(), null,
                handler: _container.GetInstance<ActionMessageHandler>(actionDescriptor),
                constraints: actionDescriptor.Route
                    .GetRouteConstraints(_constraintResolver)
                    .AddMethodConstraint(actionDescriptor.Route.Method));
        }
    }
}