using System;
using System.Net.Http;
using System.Threading.Tasks;
using Graphite;
using Graphite.Actions;
using Graphite.Monitoring;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Actions
{
    [TestFixture]
    public class ActionHandlerTests
    {
        private RequestGraph _requestGraph;
        private HttpRequestMessage _request;
        private ActionDescriptor _descriptor;
        private HttpResponseMessage _response;
        private IBehaviorChainInvoker _invoker;
        private ActionMessageHandler _messageHandler;

        [SetUp]
        public void Setup()
        {
            _requestGraph = RequestGraph.Create();
            _request = _requestGraph.GetHttpRequestMessage();
            _descriptor = _requestGraph.GetActionDescriptor();
            _response = new HttpResponseMessage();
            _invoker = Substitute.For<IBehaviorChainInvoker>();
            _messageHandler = new ActionMessageHandler(_descriptor, _invoker, 
                new Metrics(), new Configuration());
        }

        [Test]
        public async Task Should_call_invoker()
        {
            _invoker.Invoke(_descriptor, _request, _requestGraph.CancellationToken).Returns(_response);

            var result = await _messageHandler.SendAsync(_request, _requestGraph.CancellationToken);

            result.ShouldEqual(_response);
        }

        [Test]
        public async Task Should_wrap_exceptions_in_unhandled_exception()
        {
            var innerException = new Exception();
            _invoker.Invoke(_descriptor, _request, _requestGraph.CancellationToken)
                .Throws(innerException);

            var exception = await _messageHandler.Should().Throw<UnhandledGraphiteRequestException>(
                x => x.SendAsync(_request, _requestGraph.CancellationToken));

            exception.InnerException.ShouldEqual(innerException);
        }

        [Test]
        public async Task Should_not_wrap_runtime_exceptions_in_unhandled_exception()
        {
            var innerException = new GraphiteRuntimeInitializationException(
                null, _requestGraph.GetRequestContext());
            _invoker.Invoke(_descriptor, _request, _requestGraph.CancellationToken)
                .Throws(innerException);

            var exception = await _messageHandler.Should().Throw<GraphiteRuntimeInitializationException>(
                x => x.SendAsync(_request, _requestGraph.CancellationToken));

            exception.InnerException.ShouldBeNull();
        }
    }
}
