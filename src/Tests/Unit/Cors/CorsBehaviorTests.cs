using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Cors;
using System.Web.Http;
using System.Web.Http.Hosting;
using Graphite;
using Graphite.Actions;
using Graphite.Behaviors;
using Graphite.Cors;
using NSubstitute;
using NUnit.Framework;
using Should;
using Tests.Common;
using HttpMethod = System.Net.Http.HttpMethod;

namespace Tests.Unit.Cors
{
    [TestFixture]
    public class CorsBehaviorTests
    {
        private const string DefaultOrigin = "http://fark.com";

        private Configuration _configuration;
        private IBehaviorChain _behaviorChain;
        private HttpResponseMessage _innerResponse;
        private ICorsEngine _corsEngine;
        private CorsConfiguration _corsConfiguration;
        private ActionDescriptor _actionDescriptor;
        private HttpRequestMessage _requestMessage;
        private List<ICorsPolicySource> _policySources;
        private CorsBehavior _behavior;

        public class Handler
        {
            public void Get() { }
        }

        [SetUp]
        public void Setup()
        {
            _configuration = new Configuration();
            _corsEngine = new CorsEngine();
            _corsConfiguration = new CorsConfiguration();
            _actionDescriptor = new ActionDescriptor(ActionMethod.From<Handler>(x => x.Get()), 
                null, null, null, null, null, null, null);
            _innerResponse = new HttpResponseMessage();
            _behaviorChain = Substitute.For<IBehaviorChain>();
            _behaviorChain.InvokeNext().Returns(_innerResponse);
            _requestMessage = new HttpRequestMessage();
            _requestMessage.Headers.Host = "yourmom.com";
            _requestMessage.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            _policySources = new List<ICorsPolicySource>();
            _behavior = new CorsBehavior(_behaviorChain, _corsEngine, _corsConfiguration,
                _actionDescriptor, _requestMessage, _policySources, _configuration, null);
        }

        [Test]
        public async Task Should_call_next_behavior_if_no_policies_found_that_apply()
        {
            var response = await _behavior.Invoke();

            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
            response.ShouldEqual(_innerResponse);
            _innerResponse.Headers.ShouldBeEmpty();
            response.Content.ShouldBeNull();
            await _behaviorChain.ReceivedWithAnyArgs().InvokeNext();
            response.ShouldEqual(_innerResponse);
        }

        [Test]
        public async Task Should_use_first_policy_that_applies()
        {
            AppendPolicySource(x => x.AppliesWhen(a => false));
            AppendPolicySource(x => x.AllowOrigins(DefaultOrigin));
            AppendPolicySource();

            _requestMessage.Headers.Add(CorsConstants.Origin, DefaultOrigin);

            var response = await _behavior.Invoke();

            Console.WriteLine(response);

            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
            response.Headers.Count().ShouldEqual(1);
            response.Headers.GetValues(CorsConstants.AccessControlAllowOrigin)
                .ShouldOnlyContain(DefaultOrigin);
            response.Content.ShouldBeNull();
            await _behaviorChain.ReceivedWithAnyArgs().InvokeNext();
            response.ShouldEqual(_innerResponse);
        }

        [Test]
        public async Task Should_reject_non_preflight_request_if_it_has_no_origin_and_only_requests_with_origin_are_accepted()
        {
            AppendPolicySource(x => x.RejectRequestsWithoutOriginHeader());
            
            var response = await _behavior.Invoke();

            Console.WriteLine(response);

            response.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
            response.Headers.ShouldBeEmpty();
            response.Content.ShouldBeNull();
            await _behaviorChain.DidNotReceiveWithAnyArgs().InvokeNext();
        }

        [Test]
        public async Task Should_not_reject_non_preflight_request_if_it_has_no_origin_and_requests_without_origin_are_accepted()
        {
            AppendPolicySource();

            var response = await _behavior.Invoke();

            Console.WriteLine(response);

            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
            response.Headers.ShouldBeEmpty();
            response.Content.ShouldBeNull();
            await _behaviorChain.ReceivedWithAnyArgs().InvokeNext();
            response.ShouldEqual(_innerResponse);
        }

        [Test]
        public async Task Should_reject_preflight_requests_that_specify_unsupported_methods_and_are_not_configured_to_pass_through()
        {
            AppendPolicySource(x => x.AllowAnyOrigin());
            MakePreflight(_requestMessage, "FARK");

            var response = await _behavior.Invoke();

            Console.WriteLine(response);

            response.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
            response.Headers.ShouldBeEmpty();
            response.Content.ShouldBeNull();
            await _behaviorChain.DidNotReceiveWithAnyArgs().InvokeNext();
        }

        [Test]
        public async Task Should_not_reject_preflight_requests_that_specify_unsupported_methods_and_are_configured_to_pass_through()
        {
            AppendPolicySource(x => x.AllowAnyOrigin().AllowOptionRequestsToPassThrough());
            MakePreflight(_requestMessage, "FARK");

            var response = await _behavior.Invoke();

            Console.WriteLine(response);

            response.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
            response.Headers.ShouldBeEmpty();
            response.Content.ShouldBeNull();
            await _behaviorChain.ReceivedWithAnyArgs().InvokeNext();
            response.ShouldEqual(_innerResponse);
        }

        [Test]
        public async Task Should_not_reject_non_preflight_requests_that_specify_unsupported_methods()
        {
            AppendPolicySource(x => x.AllowAnyOrigin().AllowOptionRequestsToPassThrough());

            _requestMessage.Headers.Add(CorsConstants.Origin, "*");
            _requestMessage.Headers.Add(CorsConstants.AccessControlRequestMethod, "FARK");

            var response = await _behavior.Invoke();

            Console.WriteLine(response);

            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
            response.Headers.Count().ShouldEqual(1);
            response.Headers.GetValues(CorsConstants.AccessControlAllowOrigin).ShouldOnlyContain("*");
            response.Content.ShouldBeNull();
            await _behaviorChain.ReceivedWithAnyArgs().InvokeNext();
            response.ShouldEqual(_innerResponse);
        }

        [Test]
        public async Task Should_reject_preflight_requests_that_fail_validation_and_are_configured_not_to_pass_through()
        {
            AppendPolicySource(x => x.AllowOrigins("http://farker.com"));

            MakePreflight(_requestMessage);

            var response = await _behavior.Invoke();

            Console.WriteLine(response);

            response.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
            response.Headers.ShouldBeEmpty();
            response.Content.ShouldNotBeNull();
            response.Content.ReadAsStringAsync().Result
                .ShouldEqual($"The origin '{DefaultOrigin}' is not allowed.");
            await _behaviorChain.DidNotReceiveWithAnyArgs().InvokeNext();
        }

        [Test]
        public async Task Should_not_reject_preflight_requests_that_fail_validation_and_are_configured_to_pass_through()
        {
            AppendPolicySource(x => x
                .AllowOrigins("http://farker.com")
                .AllowOptionRequestsToPassThrough());

            MakePreflight(_requestMessage);

            var response = await _behavior.Invoke();

            Console.WriteLine(response);

            response.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
            response.Headers.ShouldBeEmpty();
            response.Content.ShouldNotBeNull();
            response.Content.ReadAsStringAsync().Result
                .ShouldEqual($"The origin '{DefaultOrigin}' is not allowed.");
            await _behaviorChain.ReceivedWithAnyArgs().InvokeNext();
            response.ShouldEqual(_innerResponse);
        }

        [Test]
        public async Task Should_reject_non_preflight_requests_that_fail_validation_and_are_configured_not_to_pass_through()
        {
            AppendPolicySource()
                .AllowOrigins(DefaultOrigin)
                .RejectRequestsThatFailCorsValidation();

            _requestMessage.Headers.Add(CorsConstants.Origin, "*");

            var response = await _behavior.Invoke();

            Console.WriteLine(response);

            response.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
            response.Headers.ShouldBeEmpty();
            response.Content.ShouldNotBeNull();
            response.Content.ReadAsStringAsync().Result
                .ShouldEqual("The origin '*' is not allowed.");
            await _behaviorChain.DidNotReceiveWithAnyArgs().InvokeNext();
        }

        [Test]
        public async Task Should_not_reject_non_preflight_requests_that_fail_validation_and_are_configured_to_pass_through()
        {
            AppendPolicySource().AllowOrigins(DefaultOrigin);

            _requestMessage.Headers.Add(CorsConstants.Origin, "*");

            var response = await _behavior.Invoke();

            Console.WriteLine(response);

            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
            response.Headers.ShouldBeEmpty();
            response.Content.ShouldBeNull();
            await _behaviorChain.ReceivedWithAnyArgs().InvokeNext();
            response.ShouldEqual(_innerResponse);
        }

        [Test]
        public async Task Should_return_valid_preflight_requests_when_configured_not_to_pass_through()
        {
            AppendPolicySource(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod());

            MakePreflight(_requestMessage);

            var response = await _behavior.Invoke();

            Console.WriteLine(response);

            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
            response.Headers.Count().ShouldEqual(1);
            response.Headers.GetValues(CorsConstants.AccessControlAllowOrigin).ShouldOnlyContain("*");
            response.Content.ShouldBeNull();
            await _behaviorChain.DidNotReceiveWithAnyArgs().InvokeNext();
        }

        [Test]
        public async Task Should_pass_through_valid_preflight_requests_when_configured()
        {
            AppendPolicySource(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowOptionRequestsToPassThrough());

            MakePreflight(_requestMessage);

            var response = await _behavior.Invoke();

            Console.WriteLine(response);

            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
            response.Headers.Count().ShouldEqual(1);
            response.Headers.GetValues(CorsConstants.AccessControlAllowOrigin).ShouldOnlyContain("*");
            response.Content.ShouldBeNull();
            await _behaviorChain.ReceivedWithAnyArgs().InvokeNext();
            response.ShouldEqual(_innerResponse);
        }

        [Test]
        public async Task Should_pass_through_valid_non_preflight_requests_when_configured()
        {
            AppendPolicySource(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod());

            _requestMessage.Headers.Add(CorsConstants.Origin, DefaultOrigin);

            var response = await _behavior.Invoke();

            Console.WriteLine(response);

            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
            response.Headers.Count().ShouldEqual(1);
            response.Headers.GetValues(CorsConstants.AccessControlAllowOrigin).ShouldOnlyContain("*");
            response.Content.ShouldBeNull();
            await _behaviorChain.ReceivedWithAnyArgs().InvokeNext();
            response.ShouldEqual(_innerResponse);
        }

        private CorsPolicySource AppendPolicySource(Action<CorsPolicySource> configure = null)
        {
            var policySource = CorsPolicySource.AppendPolicy(
                _corsConfiguration.PolicySources, configure);

            _policySources.Add(policySource);

            return policySource;
        }

        private static void MakePreflight(HttpRequestMessage request,
            string method = "GET", string origin = DefaultOrigin)
        {
            request.Method = HttpMethod.Options;
            request.Headers.Add(CorsConstants.Origin, origin);
            request.Headers.Add(CorsConstants.AccessControlRequestMethod, method);
        }
    }
}