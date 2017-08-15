using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Cors;
using System.Web.Http;
using Graphite.Actions;
using Graphite.Behaviors;
using Graphite.Extensibility;
using Graphite.Extensions;
using Graphite.Linq;

namespace Graphite.Cors
{
    public static class Extensions
    {
        public static ConfigurationDsl EnableCors(
            this ConfigurationDsl configuration, Action<CorsConfigurationDsl> configure = null)
        {
            var corsConfiguration = new CorsConfiguration();
            configure?.Invoke(new CorsConfigurationDsl(corsConfiguration));
            return configuration
                .ConfigureHttpRouteDecorators(x => x
                    .Append<OptionsRouteDecorator>(a => corsConfiguration.PolicySources.ThatApplyTo(a).Any()))
                .ConfigureBehaviors(x => x
                    .Append<CorsBehavior>(a => corsConfiguration.PolicySources.ThatApplyTo(a).Any())
                        .AfterOrPrepend<DefaultErrorHandlerBehavior>())
                .ConfigureRegistry(x => x
                    .Register(corsConfiguration)
                    .RegisterPlugin(corsConfiguration.CorsEngine)
                    .RegisterPlugins(corsConfiguration.PolicySources));
        }

        public static ICorsPolicySource ThatApplies(
            this IEnumerable<ICorsPolicySource> sources, CorsConfiguration corsConfiguration,
            ActionDescriptor actionDescriptor, Configuration configuration,
            HttpConfiguration httpConfiguration)
        {
            return corsConfiguration.PolicySources.ThatApply(sources,
                new ActionConfigurationContext(configuration, httpConfiguration, 
                    actionDescriptor.Action, actionDescriptor.Route))
                .FirstOrDefault();
        }

        public static CorsRequestContext GetCorsRequestContext(this HttpRequestMessage request)
        {
            var requestContext = new CorsRequestContext
            {
                RequestUri = request.RequestUri,
                HttpMethod = request.Method.Method,
                Host = request.Headers.Host,
                Origin = request.GetHeaderValue(CorsConstants.Origin),
                AccessControlRequestMethod = request.GetHeaderValue(CorsConstants.AccessControlRequestMethod)
            };

            request.GetHeaderValues(CorsConstants.AccessControlRequestHeaders)
                .Where(x => x != null)
                .SelectMany(x => x.Split(',').Select(v => v.Trim()))
                .ForEach(x => requestContext.AccessControlRequestHeaders.Add(x));

            return requestContext;
        }

        public static void WriteCorsHeaders(this HttpResponseMessage response, CorsResult corsResult)
        {
            corsResult.ToResponseHeaders()?.ForEach(x => response
                .Headers.TryAddWithoutValidation(x.Key, x.Value));
        }
    }
}
