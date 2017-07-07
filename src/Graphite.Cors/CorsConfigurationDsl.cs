using System;
using System.Web.Cors;
using Graphite.Actions;
using CorsSourcePluginDefinitions = Graphite.Extensibility.ConditionalPlugins<
    Graphite.Cors.ICorsPolicySource, Graphite.Actions.ActionConfigurationContext>;

namespace Graphite.Cors
{
    public class CorsConfigurationDsl
    {
        private readonly CorsConfiguration _configuration;

        public CorsConfigurationDsl(CorsConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Specifies the CORS engine type.
        /// </summary>
        public CorsConfigurationDsl WithEngine<T>() where T : ICorsEngine
        {
            _configuration.CorsEngine.Set<T>(true);
            return this;
        }

        /// <summary>
        /// Specifies the CORS engine instance.
        /// </summary>
        public CorsConfigurationDsl WithEngine<T>(T engine) where T : ICorsEngine
        {
            _configuration.CorsEngine.Set(engine);
            return this;
        }

        /// <summary>
        /// Configure policy sources.
        /// </summary>
        public CorsConfigurationDsl ConfigurePolicySources(Action<CorsSourcePluginDefinitions> configure)
        {
            configure(_configuration.PolicySources);
            return this;
        }

        /// <summary>
        /// Appends a policy.
        /// </summary>
        public CorsConfigurationDsl AppendPolicy(
            Action<CorsPolicySource> configure)
        {
            CorsPolicySource.AppendPolicy(_configuration.PolicySources, configure);
            return this;
        }

        /// <summary>
        /// Prepends a policy.
        /// </summary>
        public CorsConfigurationDsl PrependPolicy(
            Action<CorsPolicySource> configure)
        {
            CorsPolicySource.PrependPolicy(_configuration.PolicySources, configure);
            return this;
        }

        /// <summary>
        /// Appends a policy source type.
        /// </summary>
        public CorsConfigurationDsl AppendPolicySource<T>(
            Func<ActionConfigurationContext, bool> predicate = null) where T : ICorsPolicySource
        {
            _configuration.PolicySources.Configure(x => x.Append<T>(predicate));
            return this;
        }

        /// <summary>
        /// Preppends a policy source type.
        /// </summary>
        public CorsConfigurationDsl PrependPolicySource<T>(
            Func<ActionConfigurationContext, bool> predicate = null) where T : ICorsPolicySource
        {
            _configuration.PolicySources.Configure(x => x.Prepend<T>(predicate));
            return this;
        }

        /// <summary>
        /// Appends a policy source instance.
        /// </summary>
        public CorsConfigurationDsl AppendPolicySource<T>(T policy,
            Func<ActionConfigurationContext, bool> predicate = null) where T : ICorsPolicySource
        {
            _configuration.PolicySources.Configure(x => x.Append(policy, predicate));
            return this;
        }

        /// <summary>
        /// Preppends a policy source instance.
        /// </summary>
        public CorsConfigurationDsl PrependPolicySource<T>(T policy,
            Func<ActionConfigurationContext, bool> predicate = null) where T : ICorsPolicySource
        {
            _configuration.PolicySources.Configure(x => x.Prepend(policy, predicate));
            return this;
        }

        /// <summary>
        /// Appends the attribute policy source.
        /// </summary>
        public CorsConfigurationDsl AppendAttributePolicySource(
            Func<ActionConfigurationContext, bool> predicate = null)
        {
            _configuration.PolicySources.Configure(x => x.Append<CorsAttributePolicySource>(predicate));
            return this;
        }

        /// <summary>
        /// Preppends the attribute policy source.
        /// </summary>
        public CorsConfigurationDsl PrependAttributePolicySource(
            Func<ActionConfigurationContext, bool> predicate = null)
        {
            _configuration.PolicySources.Configure(x => x.Prepend<CorsAttributePolicySource>(predicate));
            return this;
        }
    }
}