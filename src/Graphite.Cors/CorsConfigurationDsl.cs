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

        public CorsConfigurationDsl WithEngine<T>() where T : ICorsEngine
        {
            _configuration.CorsEngine.Set<T>(true);
            return this;
        }

        public CorsConfigurationDsl WithEngine<T>(T engine) where T : ICorsEngine
        {
            _configuration.CorsEngine.Set(engine);
            return this;
        }

        public CorsConfigurationDsl ConfigurePolicySources(Action<CorsSourcePluginDefinitions> configure)
        {
            configure(_configuration.PolicySources);
            return this;
        }

        public CorsConfigurationDsl AppendPolicy(
            Action<CorsPolicySource> configure)
        {
            CorsPolicySource.AppendPolicy(_configuration.PolicySources, configure);
            return this;
        }

        public CorsConfigurationDsl PrependPolicy(
            Action<CorsPolicySource> configure)
        {
            CorsPolicySource.PrependPolicy(_configuration.PolicySources, configure);
            return this;
        }

        public CorsConfigurationDsl AppendPolicySource<T>(
            Func<ActionConfigurationContext, bool> predicate = null) where T : ICorsPolicySource
        {
            _configuration.PolicySources.Configure(x => x.Append<T>(predicate));
            return this;
        }

        public CorsConfigurationDsl PrependPolicySource<T>(
            Func<ActionConfigurationContext, bool> predicate = null) where T : ICorsPolicySource
        {
            _configuration.PolicySources.Configure(x => x.Prepend<T>(predicate));
            return this;
        }

        public CorsConfigurationDsl AppendPolicySource<T>(T policy,
            Func<ActionConfigurationContext, bool> predicate = null) where T : ICorsPolicySource
        {
            _configuration.PolicySources.Configure(x => x.Append(policy, predicate));
            return this;
        }

        public CorsConfigurationDsl PrependPolicySource<T>(T policy,
            Func<ActionConfigurationContext, bool> predicate = null) where T : ICorsPolicySource
        {
            _configuration.PolicySources.Configure(x => x.Prepend(policy, predicate));
            return this;
        }

        public CorsConfigurationDsl AppendAttributePolicySource(
            Func<ActionConfigurationContext, bool> predicate = null)
        {
            _configuration.PolicySources.Configure(x => x.Append<CorsAttributePolicySource>(predicate));
            return this;
        }

        public CorsConfigurationDsl PrependAttributePolicySource(
            Func<ActionConfigurationContext, bool> predicate = null)
        {
            _configuration.PolicySources.Configure(x => x.Prepend<CorsAttributePolicySource>(predicate));
            return this;
        }
    }
}