using System.Linq;
using System.Web.Cors;
using Graphite.Actions;
using Graphite.Cors;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Cors
{
    [TestFixture]
    public class CorsConfigurationDslTests
    {
        [Test]
        public void Should_set_cors_engine_instance()
        {
            var configuration = new CorsConfiguration();
            var engine = new CorsEngine();

            new CorsConfigurationDsl(configuration).WithEngine(engine);

            configuration.CorsEngine.HasInstance.ShouldBeTrue();
            configuration.CorsEngine.Instance.ShouldEqual(engine);
        }

        [Test]
        public void Should_set_cors_engine_type()
        {
            var configuration = new CorsConfiguration();

            new CorsConfigurationDsl(configuration).WithEngine<CorsEngine>();

            configuration.CorsEngine.HasInstance.ShouldBeFalse();
            configuration.CorsEngine.Type.ShouldEqual(typeof(CorsEngine));
        }

        [Test]
        public void Should_configure_cors_policy_sources()
        {
            var configuration = new CorsConfiguration();

            new CorsConfigurationDsl(configuration).ConfigurePolicySources(x => x
                .Configure(c => c.Append<CorsPolicySource>()));

            configuration.PolicySources.Count().ShouldEqual(1);
            var plugin = configuration.PolicySources.First();

            plugin.HasInstance.ShouldBeFalse();
            plugin.Type.ShouldEqual(typeof(CorsPolicySource));
        }

        [Test]
        public void Should_append_policy()
        {
            var configuration = new CorsConfiguration();

            new CorsConfigurationDsl(configuration)
                .AppendPolicy(x => x.PreflightMaxAge(50))
                .AppendPolicy(x => x.PreflightMaxAge(60));

            configuration.PolicySources.Count().ShouldEqual(2);

            var plugin = configuration.PolicySources.First();

            plugin.HasInstance.ShouldBeTrue();
            plugin.Type.ShouldEqual(typeof(CorsPolicySource));
            plugin.Instance.CreatePolicy().PreflightMaxAge.ShouldEqual(50);

            plugin = configuration.PolicySources.Second();

            plugin.HasInstance.ShouldBeTrue();
            plugin.Type.ShouldEqual(typeof(CorsPolicySource));
            plugin.Instance.CreatePolicy().PreflightMaxAge.ShouldEqual(60);
        }

        [Test]
        public void Should_prepend_policy()
        {
            var configuration = new CorsConfiguration();

            new CorsConfigurationDsl(configuration)
                .PrependPolicy(x => x.PreflightMaxAge(50))
                .PrependPolicy(x => x.PreflightMaxAge(60));

            configuration.PolicySources.Count().ShouldEqual(2);

            var plugin = configuration.PolicySources.First();

            plugin.HasInstance.ShouldBeTrue();
            plugin.Type.ShouldEqual(typeof(CorsPolicySource));
            plugin.Instance.CreatePolicy().PreflightMaxAge.ShouldEqual(60);

            plugin = configuration.PolicySources.Second();

            plugin.HasInstance.ShouldBeTrue();
            plugin.Type.ShouldEqual(typeof(CorsPolicySource));
            plugin.Instance.CreatePolicy().PreflightMaxAge.ShouldEqual(50);
        }

        public class Handler
        {
            public void Get() { } 
            public void Post() { }
        }

        [Test]
        public void Should_append_policy_source()
        {
            var getActionContext = new ActionConfigurationContext(null,
                null, ActionMethod.From<CorsPolicySourceTests.Handler>(x => x.Get()), null);
            var postActionContext = new ActionConfigurationContext(null,
                null, ActionMethod.From<CorsPolicySourceTests.Handler>(x => x.Post()), null);
            var configuration = new CorsConfiguration();
            var policySource = new CorsPolicySource();

            new CorsConfigurationDsl(configuration)
                .AppendPolicySource<CorsPolicySource>()
                .AppendPolicySource(policySource, x => x.ActionMethod.Name == "Get");

            configuration.PolicySources.Count().ShouldEqual(2);

            var plugin = configuration.PolicySources.First();

            plugin.HasInstance.ShouldBeFalse();
            plugin.Type.ShouldEqual(typeof(CorsPolicySource));
            plugin.AppliesTo.ShouldBeNull();

            plugin = configuration.PolicySources.Second();

            plugin.HasInstance.ShouldBeTrue();
            plugin.Type.ShouldEqual(typeof(CorsPolicySource));
            plugin.Instance.ShouldEqual(policySource);
            plugin.AppliesTo(getActionContext).ShouldBeTrue();
            plugin.AppliesTo(postActionContext).ShouldBeFalse();
        }

        [Test]
        public void Should_prepend_policy_source()
        {
            var getActionContext = new ActionConfigurationContext(null,
                null, ActionMethod.From<CorsPolicySourceTests.Handler>(x => x.Get()), null);
            var postActionContext = new ActionConfigurationContext(null,
                null, ActionMethod.From<CorsPolicySourceTests.Handler>(x => x.Post()), null);
            var configuration = new CorsConfiguration();
            var policySource = new CorsPolicySource();

            new CorsConfigurationDsl(configuration)
                .PrependPolicySource<CorsPolicySource>()
                .PrependPolicySource(policySource, x => x.ActionMethod.Name == "Get");

            configuration.PolicySources.Count().ShouldEqual(2);

            var plugin = configuration.PolicySources.First();

            plugin.HasInstance.ShouldBeTrue();
            plugin.Type.ShouldEqual(typeof(CorsPolicySource));
            plugin.Instance.ShouldEqual(policySource);
            plugin.AppliesTo(getActionContext).ShouldBeTrue();
            plugin.AppliesTo(postActionContext).ShouldBeFalse();

            plugin = configuration.PolicySources.Second();

            plugin.HasInstance.ShouldBeFalse();
            plugin.Type.ShouldEqual(typeof(CorsPolicySource));
            plugin.AppliesTo.ShouldBeNull();
        }

        [Test]
        public void Should_append_attribute_policy_source()
        {
            var getActionContext = new ActionConfigurationContext(null,
                null, ActionMethod.From<CorsPolicySourceTests.Handler>(x => x.Get()), null);
            var postActionContext = new ActionConfigurationContext(null,
                null, ActionMethod.From<CorsPolicySourceTests.Handler>(x => x.Post()), null);
            var configuration = new CorsConfiguration();

            new CorsConfigurationDsl(configuration)
                .AppendPolicySource<CorsPolicySource>()
                .AppendAttributePolicySource(x => x.ActionMethod.Name == "Get");

            configuration.PolicySources.Count().ShouldEqual(2);

            var plugin = configuration.PolicySources.First();

            plugin.HasInstance.ShouldBeFalse();
            plugin.Type.ShouldEqual(typeof(CorsPolicySource));
            plugin.AppliesTo.ShouldBeNull();

            plugin = configuration.PolicySources.Second();

            plugin.HasInstance.ShouldBeFalse();
            plugin.Type.ShouldEqual(typeof(CorsAttributePolicySource));
            plugin.AppliesTo(getActionContext).ShouldBeTrue();
            plugin.AppliesTo(postActionContext).ShouldBeFalse();
        }

        [Test]
        public void Should_prepend_attribute_policy_source()
        {
            var getActionContext = new ActionConfigurationContext(null,
                null, ActionMethod.From<CorsPolicySourceTests.Handler>(x => x.Get()), null);
            var postActionContext = new ActionConfigurationContext(null,
                null, ActionMethod.From<CorsPolicySourceTests.Handler>(x => x.Post()), null);
            var configuration = new CorsConfiguration();

            new CorsConfigurationDsl(configuration)
                .PrependPolicySource<CorsPolicySource>()
                .PrependAttributePolicySource(x => x.ActionMethod.Name == "Get");

            configuration.PolicySources.Count().ShouldEqual(2);

            var plugin = configuration.PolicySources.First();

            plugin.HasInstance.ShouldBeFalse();
            plugin.Type.ShouldEqual(typeof(CorsAttributePolicySource));
            plugin.AppliesTo(getActionContext).ShouldBeTrue();
            plugin.AppliesTo(postActionContext).ShouldBeFalse();

            plugin = configuration.PolicySources.Second();

            plugin.HasInstance.ShouldBeFalse();
            plugin.Type.ShouldEqual(typeof(CorsPolicySource));
            plugin.AppliesTo.ShouldBeNull();
        }
    }
}
