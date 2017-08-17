using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Cors;
using Graphite;
using Graphite.Actions;
using Graphite.Behaviors;
using NUnit.Framework;
using Graphite.Cors;
using Graphite.Extensions;
using Graphite.Setup;
using Should;
using Tests.Common;
using PolicyPlugin = Graphite.Extensibility.ConditionalPlugin<Graphite.Cors
    .ICorsPolicySource, Graphite.Actions.ActionConfigurationContext>;

namespace Tests.Unit.Cors
{
    [TestFixture]
    public class ExtensionTests
    {
        private Configuration _configuration;
        private ConfigurationDsl _dsl;

        [SetUp]
        public void Setup()
        {
            _configuration = new Configuration();
            _dsl = new ConfigurationDsl(_configuration, null);
        }

        public class SomeEngine : CorsEngine { }

        [Test]
        public void Should_register_cors_configuration()
        {
            _dsl.EnableCors(x => x.WithEngine<SomeEngine>());

            Should_be_instance_registration<CorsConfiguration, CorsConfiguration>();
        }

        [Test]
        public void Should_register_cors_engine_type()
        {
            _dsl.EnableCors(x => x.WithEngine<SomeEngine>());

            Should_have_type_registration<ICorsEngine, SomeEngine>(true);
        }

        [Test]
        public void Should_register_cors_engine_instance()
        {
            var corsEngine = new SomeEngine();

            _dsl.EnableCors(x => x.WithEngine(corsEngine));

            Should_be_instance_registration<ICorsEngine, SomeEngine>(corsEngine);
        }

        private void Should_be_instance_registration<TPlugin, TConcrete>(TConcrete instance = null)
            where TConcrete : class
        {
            var registration = _configuration.Registry
                .FirstOrDefault(x => x.PluginType == typeof(TPlugin));

            registration.ShouldNotBeNull();
            registration.ConcreteType.ShouldEqual(typeof(TConcrete));
            if (instance == null) registration.Instance.ShouldNotBeNull();
            else registration.Instance.ShouldEqual(instance);
            registration.IsInstance.ShouldBeTrue();
            registration.Singleton.ShouldBeFalse();
        }

        public class Handler
        {
            public void Get() { }
            public void Post() { }
        }

        [Test]
        public void Should_configure_cors_policy_sources()
        {
            var getAction = ActionMethod.From<Handler>(x => x.Get());
            var postAction = ActionMethod.From<Handler>(x => x.Post());

            _dsl.EnableCors(x => x.AppendAttributePolicySource(
                p => p.ActionMethod.Name == "Post"));

            var policySources = GetCorsConfig().PolicySources.ToList();

            policySources.Count.ShouldEqual(1);

            var policySource = policySources[0];
            policySource.AppliesTo(new ActionConfigurationContext(null,
                null, getAction, null)).ShouldBeFalse();
            policySource.AppliesTo(new ActionConfigurationContext(null,
                null, postAction, null)).ShouldBeTrue();
            Should_be_policy_source_type<CorsAttributePolicySource>(policySources[0]);

            Should_have_type_registration<ICorsPolicySource, CorsAttributePolicySource>();
        }

        private void Should_be_policy_source_type<T>(PolicyPlugin policySource) where T : ICorsPolicySource
        {
            policySource.ShouldNotBeNull();
            policySource.HasInstance.ShouldBeFalse();
            policySource.Instance.ShouldBeNull();
            policySource.Singleton.ShouldBeFalse();
            policySource.Type.ShouldEqual(typeof(T));
        }

        private void Should_have_type_registration<TPlugin, TConcrete>(bool singleton = false)
        {
            var registration = _configuration.Registry
                .FirstOrDefault(x => x.PluginType == typeof(TPlugin));

            registration.ShouldNotBeNull();
            registration.ConcreteType.ShouldEqual(typeof(TConcrete));
            registration.Instance.ShouldBeNull();
            registration.IsInstance.ShouldBeFalse();
            registration.Singleton.ShouldEqual(singleton);
        }

        [Test]
        public void Should_configure_cors_behavior()
        {
            var getAction = ActionMethod.From<Handler>(x => x.Get());
            var postAction = ActionMethod.From<Handler>(x => x.Post());

            _dsl.EnableCors(x => x.AppendAttributePolicySource(
                p => p.ActionMethod.Name == "Post"));

            _configuration.Behaviors.Count().ShouldEqual(2);

            var behavior = _configuration.Behaviors.First();
            behavior.Type.ShouldEqual(typeof(DefaultErrorHandlerBehavior));

            behavior = _configuration.Behaviors.Second();

            behavior.Type.ShouldEqual(typeof(CorsBehavior));
            behavior.AppliesTo(new ActionConfigurationContext(null,
                null, getAction, null)).ShouldBeFalse();
            behavior.AppliesTo(new ActionConfigurationContext(null,
                null, postAction, null)).ShouldBeTrue();
        }

        [Test]
        public void Should_configure_options_route_decorator()
        {
            var getAction = ActionMethod.From<Handler>(x => x.Get());
            var postAction = ActionMethod.From<Handler>(x => x.Post());

            _dsl.EnableCors(x => x.AppendAttributePolicySource(
                p => p.ActionMethod.Name == "Post"));

            _configuration.HttpRouteDecorators.Count().ShouldEqual(1);

            var decorator = _configuration.HttpRouteDecorators.First();

            decorator.Type.ShouldEqual(typeof(OptionsRouteDecorator));
            decorator.AppliesTo(new ActionConfigurationContext(null,
                null, getAction, null)).ShouldBeFalse();
            decorator.AppliesTo(new ActionConfigurationContext(null,
                null, postAction, null)).ShouldBeTrue();
        }

        private CorsConfiguration GetCorsConfig()
        {
            return _configuration.Registry
                .FirstOrDefault(x => x.PluginType.IsType<CorsConfiguration>())
                .Instance.As<CorsConfiguration>();
        }

        [Test]
        public void Should_return_policy_sources_that_apply_in_order()
        {
            var policy1 = new CorsPolicySource();
            var policy2 = new CorsPolicySource();
            var policy3 = new CorsPolicySource();

            var corsConfiguration = new CorsConfiguration();

            corsConfiguration.PolicySources.Configure(c => c
                .Append(policy1, p => p.ActionMethod.Name == "Get")
                .Append(policy2)
                .Append(policy3));

            var sources = new List<ICorsPolicySource>
            {
                policy2, policy3, policy1
            };

            var applies = sources.ThatApplies(corsConfiguration, new ActionDescriptor(
                ActionMethod.From<Handler>(x => x.Post()), null, 
                    null, null, null, null, null, null), null, null);

            applies.ShouldEqual(policy2);
        }

        [Test]
        public void Should_get_cors_request_context_for_non_cors_request()
        {
            var context = new HttpRequestMessage().GetCorsRequestContext();

            context.RequestUri.ShouldBeNull();
            context.HttpMethod.ShouldEqual("GET");
            context.Origin.ShouldBeNull();
            context.Host.ShouldBeNull();
            context.AccessControlRequestMethod.ShouldBeNull();
            context.AccessControlRequestHeaders.ShouldBeEmpty();
            context.Properties.ShouldBeEmpty();
            context.IsPreflight.ShouldBeFalse();
        }

        [TestCase("GET", false)]
        [TestCase("OPTIONS", true)]
        public void Should_get_cors_request_context_non_cors_request(
            string method, bool preflight)
        {
            var requestMessage = new HttpRequestMessage(new HttpMethod(method), new Uri("http://fark.com/farker"));
            requestMessage.Headers.Host = "fark.com";
            requestMessage.Headers.Add(CorsConstants.Origin, "http://yourmom.com");
            requestMessage.Headers.Add(CorsConstants.AccessControlRequestMethod, "POST");
            requestMessage.Headers.Add(CorsConstants.AccessControlRequestHeaders, "header1,header2");

            var context = requestMessage.GetCorsRequestContext();

            context.RequestUri.ShouldEqual(requestMessage.RequestUri);
            context.HttpMethod.ShouldEqual(requestMessage.Method.Method);
            context.Host.ShouldEqual(requestMessage.Headers.Host);
            context.Origin.ShouldEqual("http://yourmom.com");
            context.AccessControlRequestMethod.ShouldEqual("POST");
            context.AccessControlRequestHeaders.ShouldOnlyContain("header1", "header2");
            context.Properties.ShouldBeEmpty();
            context.IsPreflight.ShouldEqual(preflight);
        }

        [Test]
        public void Should_write_cors_headers_from_empty_result()
        {
            var response = new HttpResponseMessage();

            response.WriteCorsHeaders(new CorsResult());

            response.Headers.ShouldBeEmpty();
        }

        [Test]
        public void Should_write_cors_headers()
        {
            var corsResult = new CorsResult
            {
                AllowedMethods = { "PUT", "PATCH" }
            };

            var response = new HttpResponseMessage();

            response.WriteCorsHeaders(corsResult);

            response.Headers.GetValues(CorsConstants.AccessControlAllowMethods)
                .ShouldOnlyContain("PUT,PATCH");
        }
    }
}
