using Graphite.Http;
using Graphite.Linq;
using Graphite.Routing;

namespace Graphite.Cors
{
    public class OptionsRouteDecorator : IHttpRouteDecorator
    {
        public bool AppliesTo(HttpRouteDecoratorContext context)
        {
            return !context.Route.MethodConstraints.ContainsUncase(HttpMethod.Options.Method);
        }

        public void Decorate(HttpRouteDecoratorContext context)
        {
            context.Route.MethodConstraints.Add(HttpMethod.Options.Method);
        }
    }
}
