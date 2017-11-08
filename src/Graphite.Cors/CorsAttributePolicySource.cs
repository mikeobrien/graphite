using System;
using System.Collections.Generic;
using System.Linq;
using Graphite.Actions;
using Graphite.Extensions;

namespace Graphite.Cors
{
    public class CorsAttributePolicySource : ICorsPolicySource
    {
        private static readonly Func<ActionMethod, GraphiteCorsPolicy> PolicyCache =
            Memoize.Func<ActionMethod, GraphiteCorsPolicy>(CreatePolicy);
        private readonly ActionDescriptor _action;

        public CorsAttributePolicySource(ActionDescriptor action)
        {
            _action = action;
        }

        public bool Applies()
        {
            return !_action.Action.HasActionOrHandlerAttribute<OverrideCorsAttribute>() &&
                (_action.Action.HasActionOrHandlerAttribute<CorsExposedHeadersAttribute>() ||
                _action.Action.HasActionOrHandlerAttribute<CorsAllowedHeadersAttribute>() ||
                _action.Action.HasActionOrHandlerAttribute<CorsAllowedMethodsAttribute>() ||
                _action.Action.HasActionOrHandlerAttribute<CorsAllowedOriginsAttribute>() ||
                _action.Action.HasActionOrHandlerAttribute<CorsAttribute>());
        }

        public GraphiteCorsPolicy CreatePolicy()
        {
            return PolicyCache(_action.Action);
        }

        private static GraphiteCorsPolicy CreatePolicy(ActionMethod action)
        {
            var policy = new GraphiteCorsPolicy();

            Add<CorsExposedHeadersAttribute>(policy.ExposedHeaders, action, x => x.Headers);
            Add<CorsAllowedHeadersAttribute>(policy.Headers, action, x => x.Headers);
            Add<CorsAllowedMethodsAttribute>(policy.Methods, action, x => x.Methods);
            Add<CorsAllowedOriginsAttribute>(policy.Origins, action, x => x.Origins);

            var corsAttribute = action.GetActionOrHandlerAttribute<CorsAttribute>();

            if (corsAttribute != null)
            {
                policy.AllowOptionRequestsToPassThrough =
                    corsAttribute.AllowOptionRequestsToPassThrough;
                policy.AllowRequestsWithoutOriginHeader =
                    corsAttribute.AllowRequestsWithoutOriginHeader;
                policy.AllowRequestsThatFailCors =
                    corsAttribute.AllowRequestsThatFailCors;
                policy.AllowAnyHeader = corsAttribute.AllowAnyHeader;
                policy.AllowAnyMethod = corsAttribute.AllowAnyMethod;
                policy.AllowAnyOrigin = corsAttribute.AllowAnyOrigin;
                policy.PreflightMaxAge = corsAttribute.PreflightMaxAge;
                policy.SupportsCredentials = corsAttribute.SupportsCredentials;
            }

            return policy;
        }

        private static void Add<T>(IList<string> source, ActionMethod action, 
            Func<T, string[]> items) where T : Attribute
        {
            var attribute = action.GetActionOrHandlerAttribute<T>();

            if (attribute != null && items(attribute).Any())
                source.AddRange(items(attribute));
        }
    }
}