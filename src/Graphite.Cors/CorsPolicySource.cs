using System;
using Graphite.Actions;
using Graphite.Extensibility;
using Graphite.Extensions;
using CorsSourcePluginDefinitions = Graphite.Extensibility.PluginDefinitions<
    Graphite.Cors.ICorsPolicySource,
    Graphite.Actions.ActionConfigurationContext>;

namespace Graphite.Cors
{
    public class CorsPolicySource : ICorsPolicySource
    {
        private readonly GraphiteCorsPolicy _corsPolicy = new GraphiteCorsPolicy();
        private Func<ActionConfigurationContext, bool> _applies;

        public static CorsPolicySource AppendPolicy(
            CorsSourcePluginDefinitions definitions,
            Action<CorsPolicySource> configure)
        {
            return AddPolicy(configure, (po, pr) => definitions.Append(po, pr));
        }

        public static CorsPolicySource PrependPolicy(
            CorsSourcePluginDefinitions definitions,
            Action<CorsPolicySource> configure)
        {
            return AddPolicy(configure, (po, pr) => definitions.Prepend(po, pr));
        }

        private static CorsPolicySource AddPolicy(
            Action<CorsPolicySource> configure,
            Action<CorsPolicySource, Func<ActionConfigurationContext, bool>> add)
        {
            var policy = new CorsPolicySource();
            configure?.Invoke(policy);
            add(policy, x => !x.ActionMethod.HasActionOrHandlerAttribute
                <OverrideCorsAttribute>() && (policy._applies?.Invoke(x) ?? true));
            return policy;
        }

        bool IConditional.Applies()
        {
            return true;
        }

        GraphiteCorsPolicy ICorsPolicySource.CreatePolicy()
        {
            return _corsPolicy;
        }

        public CorsPolicySource AppliesWhen(Func<ActionConfigurationContext, bool> applies)
        {
            _applies = applies;
            return this;
        }

        public CorsPolicySource AllowOptionRequestsToPassThrough()
        {
            _corsPolicy.AllowOptionRequestsToPassThrough = true;
            return this;
        }

        public CorsPolicySource RejectRequestsWithoutOriginHeader()
        {
            _corsPolicy.AllowRequestsWithoutOriginHeader = false;
            return this;
        }

        public CorsPolicySource RejectRequestsThatFailCorsValidation()
        {
            _corsPolicy.AllowRequestsThatFailCors = false;
            return this;
        }

        public CorsPolicySource AllowAnyMethod()
        {
            _corsPolicy.AllowAnyMethod = true;
            return this;
        }

        public CorsPolicySource AllowAnyHeader()
        {
            _corsPolicy.AllowAnyHeader = true;
            return this;
        }

        public CorsPolicySource AllowAnyOrigin()
        {
            _corsPolicy.AllowAnyOrigin = true;
            return this;
        }

        public CorsPolicySource SupportsCredentials()
        {
            _corsPolicy.SupportsCredentials = true;
            return this;
        }

        public CorsPolicySource PreflightMaxAge(long? age)
        {
            _corsPolicy.PreflightMaxAge = age;
            return this;
        }

        public CorsPolicySource AllowExposedHeaders(params string[] headers)
        {
            _corsPolicy.ExposedHeaders.AddRange(headers);
            return this;
        }

        public CorsPolicySource AllowHeaders(params string[] headers)
        {
            _corsPolicy.Headers.AddRange(headers);
            return this;
        }

        public CorsPolicySource AllowMethods(params string[] methods)
        {
            _corsPolicy.Methods.AddRange(methods);
            return this;
        }

        public CorsPolicySource AllowOrigins(params string[] origins)
        {
            _corsPolicy.Origins.AddRange(origins);
            return this;
        }
    }
}