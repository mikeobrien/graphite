using Graphite.Extensibility;

namespace Graphite.Routing
{
    public class HttpRouteDecoratorContext
    {
        public HttpRouteDecoratorContext(HttpRouteConfiguration route)
        {
            Route = route;
        }

        public HttpRouteConfiguration Route { get; }
    }

    public interface IHttpRouteDecorator : IConditional<HttpRouteDecoratorContext>
    {
        void Decorate(HttpRouteDecoratorContext route);
    }
}