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
            var innerBehavior = Substitute.For<IBehavior>();
            innerBehavior.Invoke().Returns(responseMessage);

            var result = await new DefaultErrorHandlerBehavior(
                new Configuration(), innerBehavior, new HttpRequestMessage(),
                new HttpResponseMessage()).Invoke();

            result.ShouldEqual(responseMessage);
        }

        [Test]
        public async Task Should_return_bad_request_when_bad_request_exception_thrown()
        {
            var innerBehavior = Substitute.For<IBehavior>();
            innerBehavior.Invoke().Throws(new BadRequestException("fark"));

            var result = await new DefaultErrorHandlerBehavior(
                new Configuration(), innerBehavior, new HttpRequestMessage(),
                new HttpResponseMessage()).Invoke();

            result.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
            result.ReasonPhrase.ShouldEqual("fark");
        }

        [Test]
        public async Task Should_return_internal_server_error_for_unhandled_exceptions(
            [Values(true, false)] bool diagnosticsEnabled)
        {
            var configuration = new Configuration
            {
                Diagnostics = diagnosticsEnabled,
                UnhandledExceptionStatusText = "fark"
            };
            var innerBehavior = Substitute.For<IBehavior>();
            innerBehavior.Invoke().Throws<Exception>();

            var result = await new DefaultErrorHandlerBehavior(
                configuration, innerBehavior, new HttpRequestMessage(), 
                new HttpResponseMessage()).Invoke();

            result.StatusCode.ShouldEqual(HttpStatusCode.InternalServerError);
            result.ReasonPhrase.ShouldEqual("fark");

            if (diagnosticsEnabled) result.Content.ReadAsStringAsync()
                .Result.ShouldStartWith("Method: GET, RequestUri: '<null>'");
            else result.Content.ShouldBeNull();
        }
    }
}
