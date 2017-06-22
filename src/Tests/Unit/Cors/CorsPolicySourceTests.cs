using System.Linq;
using Graphite.Actions;
using Graphite.Cors;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Cors
{
    [TestFixture]
    public class CorsPolicySourceTests
    {
        [Test]
        public void Should_build_policy()
        {
            var policySource = new CorsPolicySource()
                .AllowHeaders("header1", "header2")
                .AllowMethods("method1", "method2")
                .AllowOrigins("origin1", "origin2")
                .AllowExposedHeaders("exposed-header1", "exposed-header2")
                .PreflightMaxAge(50)
                .SupportsCredentials()
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod()
                .RejectRequestsThatFailCorsValidation()
                .RejectRequestsWithoutOriginHeader()
                .AllowOptionRequestsToPassThrough() as ICorsPolicySource;

            var policy = policySource.CreatePolicy();

            policy.AllowOptionRequestsToPassThrough.ShouldBeTrue();
            policy.AllowRequestsThatFailCors.ShouldBeFalse();
            policy.AllowRequestsWithoutOriginHeader.ShouldBeFalse();
            policy.AllowAnyHeader.ShouldBeTrue();
            policy.AllowAnyMethod.ShouldBeTrue();
            policy.AllowAnyOrigin.ShouldBeTrue();
            policy.SupportsCredentials.ShouldBeTrue();
            policy.PreflightMaxAge.ShouldEqual(50);
            policy.ExposedHeaders.ShouldOnlyContain("exposed-header1", "exposed-header2");
            policy.Headers.ShouldOnlyContain("header1", "header2");
            policy.Methods.ShouldOnlyContain("method1", "method2");
            policy.Origins.ShouldOnlyContain("origin1", "origin2");
        }

        public class Handler
        {
            public void Post() { }
            public void Get() { }
            [OverrideCors]
            public void Override() { }
        }

        [Test]
        public void Should_append_policy_source_plugin()
        {
            var getActionContext = new ActionConfigurationContext(null, 
                ActionMethod.From<Handler>(x => x.Get()), null);
            var postActionContext = new ActionConfigurationContext(null,
                ActionMethod.From<Handler>(x => x.Post()), null);
            var overrideActionContext = new ActionConfigurationContext(null,
                ActionMethod.From<Handler>(x => x.Override()), null);
            var configuration = new CorsConfiguration();

            CorsPolicySource.AppendPolicy(configuration.PolicySources, x => x
                .PreflightMaxAge(1));
            CorsPolicySource.AppendPolicy(configuration.PolicySources, x => x
                .PreflightMaxAge(2)
                .AppliesWhen(a => a.ActionMethod.Name == "Get"));

            configuration.PolicySources.Count().ShouldEqual(2);

            var plugin = configuration.PolicySources.First();

            plugin.AppliesTo(getActionContext).ShouldBeTrue();
            plugin.AppliesTo(postActionContext).ShouldBeTrue();
            plugin.AppliesTo(overrideActionContext).ShouldBeFalse();
            plugin.Instance.CreatePolicy().PreflightMaxAge.ShouldEqual(1);

            plugin = configuration.PolicySources.Second();
            
            plugin.AppliesTo(getActionContext).ShouldBeTrue();
            plugin.AppliesTo(postActionContext).ShouldBeFalse();
            plugin.AppliesTo(overrideActionContext).ShouldBeFalse();
            plugin.Instance.CreatePolicy().PreflightMaxAge.ShouldEqual(2);
        }

        [Test]
        public void Should_prepend_policy_source_plugin()
        {
            var getActionContext = new ActionConfigurationContext(null,
                ActionMethod.From<Handler>(x => x.Get()), null);
            var postActionContext = new ActionConfigurationContext(null,
                ActionMethod.From<Handler>(x => x.Post()), null);
            var overrideActionContext = new ActionConfigurationContext(null,
                ActionMethod.From<Handler>(x => x.Override()), null);
            var configuration = new CorsConfiguration();

            CorsPolicySource.PrependPolicy(configuration.PolicySources, x => x
                .PreflightMaxAge(1));
            CorsPolicySource.PrependPolicy(configuration.PolicySources, x => x
                .PreflightMaxAge(2)
                .AppliesWhen(a => a.ActionMethod.Name == "Get"));

            configuration.PolicySources.Count().ShouldEqual(2);

            var plugin = configuration.PolicySources.First();

            plugin.AppliesTo(getActionContext).ShouldBeTrue();
            plugin.AppliesTo(postActionContext).ShouldBeFalse();
            plugin.AppliesTo(overrideActionContext).ShouldBeFalse();
            plugin.Instance.CreatePolicy().PreflightMaxAge.ShouldEqual(2);

            plugin = configuration.PolicySources.Second();

            plugin.AppliesTo(getActionContext).ShouldBeTrue();
            plugin.AppliesTo(postActionContext).ShouldBeTrue();
            plugin.AppliesTo(overrideActionContext).ShouldBeFalse();
            plugin.Instance.CreatePolicy().PreflightMaxAge.ShouldEqual(1);
        }
    }
}
