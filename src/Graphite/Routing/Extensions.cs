using System.Collections.Generic;
using Graphite.Actions;
using Graphite.Extensibility;

namespace Graphite.Routing
{
    public static class Extensions
    {
        public static IEnumerable<IRouteConvention> ThatApplyTo(
            this IEnumerable<IRouteConvention> routeConventions,
            ActionMethod actionMethod, ActionSourceContext context, 
            Configuration configuration)
        {
            return configuration.RouteConventions.ThatApplyTo(routeConventions,
                new RouteContext(configuration, context.HttpConfiguration, actionMethod));
        }

        public static IEnumerable<IUrlConvention> ThatApplyTo(
            this IEnumerable<IUrlConvention> urlConventions,
            UrlContext urlContext, Configuration configuration)
        {
            return configuration.UrlConventions.ThatApplyTo(urlConventions, urlContext);
        }

        public static List<RouteDescriptor> GetRouteDescriptors(
            this IRouteConvention routeConvention,
            ActionSourceContext context, ActionMethod actionMethod)

        {
            return routeConvention.GetRouteDescriptors(new RouteContext(
                context.Configuration, context.HttpConfiguration, actionMethod));
        }
    }
}