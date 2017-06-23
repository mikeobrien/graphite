using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Graphite;
using Graphite.Behaviors;
using Graphite.Binding;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Should;

namespace Tests.Unit
{
    [TestFixture]
    public class DefaultErrorHandlerBehaviorTests
    {
        [Test]
        public async Task Should_call_inner_behavior()
        {
            var responseMessage = new HttpResponseMessage();
            var behaviorChain = Substitute.For<IBehaviorChain>();
            behaviorChain.InvokeNext().Returns(responseMessage);

            var result = await new DefaultErrorHandlerBehavior(
                new Configuration(), behaviorChain, new HttpRequestMessage(),
                new HttpResponseMessage()).Invoke();

            result.ShouldEqual(responseMessage);
        }

        [Test]
        public async Task Should_return_bad_request_when_bad_request_exception_thrown()
        {
            var behaviorChain = Substitute.For<IBehaviorChain>();
            behaviorChain.InvokeNext().Throws(new BadRequestException("fark"));

            var result = await new DefaultErrorHandlerBehavior(
                new Configuration(), behaviorChain, new HttpRequestMessage(),
                new HttpResponseMessage()).Invoke();

            result.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
            result.ReasonPhrase.ShouldEqual("fark");
        }

        [Test]
        public async Task Should_return_internal_server_error_for_unhandled_exceptions(
            [Values(true, false)] bool returnErrorMessages)
        {
            var configuration = new Configuration
            {
                ReturnErrorMessage = returnErrorMessages,
                UnhandledExceptionStatusText = "fark"
            };
            var behaviorChain = Substitute.For<IBehaviorChain>();
            behaviorChain.InvokeNext().Throws<Exception>();

            var result = await new DefaultErrorHandlerBehavior(
                configuration, behaviorChain, new HttpRequestMessage(), 
                new HttpResponseMessage()).Invoke();

            result.StatusCode.ShouldEqual(HttpStatusCode.InternalServerError);
            result.ReasonPhrase.ShouldEqual("fark");

            if (returnErrorMessages) result.Content.ReadAsStringAsync()
                .Result.ShouldStartWith("Method: GET, RequestUri: '<null>'");
            else result.Content.ShouldBeNull();
        }
    }
}
