using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Routing;
using System.Web.Http.Routing.Constraints;
using Graphite.Actions;
using Graphite.Extensibility;
using Graphite.Extensions;
using Graphite.Linq;
using HttpMethod = System.Net.Http.HttpMethod;

namespace Graphite.Routing
{
    public static class Extensions
    {
        public static IEnumerable<IRouteConvention> ThatApplyTo(
            this IEnumerable<IRouteConvention> routeConventions,
            ActionMethod actionMethod, Configuration configuration,
            HttpConfiguration httpConfiguration)
        {
            return configuration.RouteConventions
                .ThatAppliesTo(routeConventions, new RouteConfigurationContext(
                    configuration, httpConfiguration, actionMethod), 
                        new RouteContext(actionMethod));
        }

        public static IEnumerable<IUrlConvention> ThatApplyTo(
            this IEnumerable<IUrlConvention> urlConventions,
            UrlContext urlContext, Configuration configuration, 
            HttpConfiguration httpConfiguration)
        {
            return configuration.UrlConventions.ThatAppliesTo(urlConventions, 
                new UrlConfigurationContext(configuration, httpConfiguration, 
                    urlContext), urlContext);
        }


        public static IEnumerable<IHttpRouteDecorator> ThatApplyTo(
            this IEnumerable<IHttpRouteDecorator> routeDecorators,
            ActionDescriptor actionDescriptor,
            HttpRouteConfiguration routeConfiguration, 
            Configuration configuration, 
            HttpConfiguration httpConfiguration)
        {
            return configuration.HttpRouteDecorators
                .ThatAppliesTo(routeDecorators,
                    new ActionConfigurationContext(configuration, httpConfiguration, 
                        actionDescriptor.Action, actionDescriptor.Route),
                    new HttpRouteDecoratorContext(routeConfiguration));
        }

        public static void Decorate(this IEnumerable<IHttpRouteDecorator>
            routeDecorators, HttpRouteConfiguration routeConfiguration)
        {
            var context = new HttpRouteDecoratorContext(routeConfiguration);
            routeDecorators.ForEach(x => x.Decorate(context));
        }

        public static List<RouteDescriptor> GetRouteDescriptors(
            this IRouteConvention routeConvention, ActionMethod actionMethod)

        {
            return routeConvention.GetRouteDescriptors(new RouteContext(actionMethod));
        }

        // R&D'd from the Web Api source code
        public const string ParameterNameRegex = @"(?<parameterName>.+?)";
        public const string ConstraintRegex = @"(:(?<constraint>.*?(\(.*?\))?))*";

        public static readonly Regex ParameterRegex = new Regex(
            $"{{{ParameterNameRegex}{ConstraintRegex}}}", RegexOptions.Compiled);

        public static string RemoveConstraints(this string routeTemplate)
        {
            return ParameterRegex.Replace(routeTemplate, @"{${parameterName}}");
        }

        public static string RemoveParameterNames(this string routeTemplate)
        {
            return ParameterRegex.Replace(routeTemplate, x => "{" + x.Groups["constraint"]
                .Captures.Cast<Capture>().Select(y => $":{y.Value}").OrderBy(y => y).Join() + "}");
        }

        public static Dictionary<string, object> GetRouteConstraints(this 
            RouteDescriptor routeDescriptor, IInlineConstraintResolver resolver)
        {
            var constraints = new Dictionary<string, object>();
            ParameterRegex.Matches(routeDescriptor.Url).Cast<Match>().ForEach(x =>
            {
                var parameterName = x.Groups["parameterName"].Value.TrimStart('*');
                var constraintDefinitions = x.Groups["constraint"].Captures.Cast<Capture>()
                    .Select(c => resolver.ResolveConstraint(c.Value))
                    .Where(c => c != null).ToList();
                if (constraintDefinitions.Any())
                    constraints.Add(parameterName, constraintDefinitions.Count == 1
                        ? constraintDefinitions.First()
                        : new CompoundRouteConstraint(constraintDefinitions));
            });
            return constraints;
        }

        public const string HttpMethodConstraintName = "$httpMethod";

        public static Dictionary<string, object> AddMethodConstraints(
            this Dictionary<string, object> constraints, IEnumerable<string> methods)
        {
            constraints.Add(HttpMethodConstraintName, new HttpMethodConstraint(
                methods.Select(x => new HttpMethod(x)).ToArray()));
            return constraints;
        }
    }
}