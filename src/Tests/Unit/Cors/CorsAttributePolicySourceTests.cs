using Graphite.Actions;
using Graphite.Cors;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Cors
{
    [TestFixture]
    public class CorsAttributePolicySourceTests
    {
        public class NoAttributesHandler
        {
            public void Get() { }
        }

        [Test]
        public void Should_not_apply_when_no_attributes_applied()
        {
            new CorsAttributePolicySource(new ActionDescriptor(
                ActionMethod.From<NoAttributesHandler>(x => x.Get()), null))
                .Applies().ShouldBeFalse();
        }

        [Test]
        public void Should_get_cors_config_when_no_attributes_applied()
        {
            var policy = new CorsAttributePolicySource(new ActionDescriptor(
                ActionMethod.From<NoAttributesHandler>(x => x.Get()), null)).CreatePolicy();

            policy.AllowOptionRequestsToPassThrough.ShouldBeFalse();
            policy.AllowRequestsWithoutOriginHeader.ShouldBeTrue();
            policy.AllowRequestsThatFailCors.ShouldBeTrue();
            policy.AllowAnyHeader.ShouldBeFalse();
            policy.AllowAnyMethod.ShouldBeFalse();
            policy.AllowAnyOrigin.ShouldBeFalse();
            policy.PreflightMaxAge.ShouldBeNull();
            policy.SupportsCredentials.ShouldBeFalse();
            policy.ExposedHeaders.ShouldBeEmpty();
            policy.Headers.ShouldBeEmpty();
            policy.Methods.ShouldBeEmpty();
            policy.Origins.ShouldBeEmpty();
        }

        public class ActionAttributesHandler
        {
            [CorsExposedHeaders("exposed-header1", "exposed-header2")]
            [CorsAllowedHeaders("allowed-header1", "allowed-header2")]
            [CorsAllowedMethods("put", "patch")]
            [CorsAllowedOrigins("fark.com", "farker.com")]
            [Cors(true, true, true, 50, true, true, false, false)]
            public void Get() { }
        }

        [Test]
        public void Should_apply_when_attributes_applied_to_action()
        {
            new CorsAttributePolicySource(new ActionDescriptor(
                    ActionMethod.From<ActionAttributesHandler>(x => x.Get()), null))
                .Applies().ShouldBeTrue();
        }

        [Test]
        public void Should_get_cors_config_from_attributes_applied_to_action()
        {
            var policy = new CorsAttributePolicySource(new ActionDescriptor(
                ActionMethod.From<ActionAttributesHandler>(x => x.Get()), null)).CreatePolicy();

            policy.AllowOptionRequestsToPassThrough.ShouldBeTrue();
            policy.AllowRequestsWithoutOriginHeader.ShouldBeFalse();
            policy.AllowRequestsThatFailCors.ShouldBeFalse();
            policy.AllowAnyHeader.ShouldBeTrue();
            policy.AllowAnyMethod.ShouldBeTrue();
            policy.AllowAnyOrigin.ShouldBeTrue();
            policy.PreflightMaxAge.ShouldEqual(50);
            policy.SupportsCredentials.ShouldBeTrue();
            policy.ExposedHeaders.ShouldOnlyContain("exposed-header1", "exposed-header2");
            policy.Headers.ShouldOnlyContain("allowed-header1", "allowed-header2");
            policy.Methods.ShouldOnlyContain("put", "patch");
            policy.Origins.ShouldOnlyContain("fark.com", "farker.com");
        }

        [CorsExposedHeaders("exposed-header1", "exposed-header2")]
        [CorsAllowedHeaders("allowed-header1", "allowed-header2")]
        [CorsAllowedMethods("put", "patch")]
        [CorsAllowedOrigins("fark.com", "farker.com")]
        [Cors(true, true, true, 50, true, true, false, false)]
        public class HandlerAttributesHandler
        {
            public void Get() { }
        }

        [Test]
        public void Should_apply_when_attributes_applied_to_handler()
        {
            new CorsAttributePolicySource(new ActionDescriptor(
                    ActionMethod.From<HandlerAttributesHandler>(x => x.Get()), null))
                .Applies().ShouldBeTrue();
        }

        [Test]
        public void Should_get_cors_config_from_attributes_applied_to_handler()
        {
            var policy = new CorsAttributePolicySource(new ActionDescriptor(
                ActionMethod.From<HandlerAttributesHandler>(x => x.Get()), null)).CreatePolicy();

            policy.AllowOptionRequestsToPassThrough.ShouldBeTrue();
            policy.AllowRequestsWithoutOriginHeader.ShouldBeFalse();
            policy.AllowRequestsThatFailCors.ShouldBeFalse();
            policy.AllowAnyHeader.ShouldBeTrue();
            policy.AllowAnyMethod.ShouldBeTrue();
            policy.AllowAnyOrigin.ShouldBeTrue();
            policy.PreflightMaxAge.ShouldEqual(50);
            policy.SupportsCredentials.ShouldBeTrue();
            policy.ExposedHeaders.ShouldOnlyContain("exposed-header1", "exposed-header2");
            policy.Headers.ShouldOnlyContain("allowed-header1", "allowed-header2");
            policy.Methods.ShouldOnlyContain("put", "patch");
            policy.Origins.ShouldOnlyContain("fark.com", "farker.com");
        }

        [CorsExposedHeaders("exposed-header1", "exposed-header2")]
        [CorsAllowedHeaders("allowed-header1", "allowed-header2")]
        [CorsAllowedMethods("put", "patch")]
        [CorsAllowedOrigins("fark.com", "farker.com")]
        [Cors(false, false, false, 60)]
        public class OverrideAttributesHandler
        {
            [CorsExposedHeaders("action-exposed-header1", "action-exposed-header2")]
            [CorsAllowedHeaders("action-allowed-header1", "action-allowed-header2")]
            [CorsAllowedMethods("action-put", "action-patch")]
            [CorsAllowedOrigins("action-fark.com", "action-farker.com")]
            [Cors(true, true, true, 60, true, true, false, false)]
            public void Get() { }

            [OverrideCors]
            public void GetOverride() { }
        }

        [Test]
        public void Should_override_cors_config_on_action()
        {
            var policy = new CorsAttributePolicySource(new ActionDescriptor(
                ActionMethod.From<OverrideAttributesHandler>(x => x.Get()), null)).CreatePolicy();

            policy.AllowOptionRequestsToPassThrough.ShouldBeTrue();
            policy.AllowRequestsWithoutOriginHeader.ShouldBeFalse();
            policy.AllowRequestsThatFailCors.ShouldBeFalse();
            policy.AllowAnyHeader.ShouldBeTrue();
            policy.AllowAnyMethod.ShouldBeTrue();
            policy.AllowAnyOrigin.ShouldBeTrue();
            policy.PreflightMaxAge.ShouldEqual(60);
            policy.SupportsCredentials.ShouldBeTrue();
            policy.ExposedHeaders.ShouldOnlyContain("action-exposed-header1", "action-exposed-header2");
            policy.Headers.ShouldOnlyContain("action-allowed-header1", "action-allowed-header2");
            policy.Methods.ShouldOnlyContain("action-put", "action-patch");
            policy.Origins.ShouldOnlyContain("action-fark.com", "action-farker.com");
        }

        [Test]
        public void Should_not_apply_when_overriden()
        {
            new CorsAttributePolicySource(new ActionDescriptor(
                    ActionMethod.From<OverrideAttributesHandler>(x => x.GetOverride()), null))
                .Applies().ShouldBeFalse();
        }
    }
}
