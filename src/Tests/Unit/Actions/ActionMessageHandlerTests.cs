using System;
using System.Net.Http;
using System.Threading.Tasks;
using Graphite;
using Graphite.Actions;
using Graphite.Behaviors;
using Graphite.Monitoring;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Actions
{
    [TestFixture]
    public class ActionMessageHandlerTests
    {
        private RequestGraph _requestGraph;
        private HttpRequestMessage _request;
        private ActionDescriptor _descriptor;
        private HttpResponseMessage _response;
        private IBehaviorChainInvoker _invoker;
        private IUnhandledExceptionHandler _unhandledExceptionHandler;
        private ActionMessageHandler _messageHandler;

        [SetUp]
        public void Setup()
        {
            _requestGraph = RequestGraph.Create();
            _request = _requestGraph.GetHttpRequestMessage();
            _descriptor = _requestGraph.GetActionDescriptor();
            _response = new HttpResponseMessage();
            _invoker = Substitute.For<IBehaviorChainInvoker>();
            _unhandledExceptionHandler = Substitute.For<IUnhandledExceptionHandler>();
            _messageHandler = new ActionMessageHandler(new Configuration(), 
                _descriptor, _unhandledExceptionHandler, _invoker, new Metrics());
        }

        [Test]
        public async Task Should_call_invoker()
        {
            _invoker.Invoke(_descriptor, _request, _requestGraph.CancellationToken).Returns(_response);

            var result = await _messageHandler.SendAsync(_request, _requestGraph.CancellationToken);

            result.ShouldEqual(_response);
        }

        [Test]
        public async Task Should_call_unhandled_exception_handler_when_exception_is_thrown()
        {
            var exceptionResponse = new HttpResponseMessage();
            var innerException = new Exception();
            _invoker.Invoke(_descriptor, _request, _requestGraph.CancellationToken)
                .Throws(innerException);
            _unhandledExceptionHandler.HandleException(innerException, _descriptor, _request)
                .Returns(exceptionResponse);

            var response = await _messageHandler.SendAsync(_request, _requestGraph.CancellationToken);

            response.ShouldEqual(exceptionResponse);
        }
    }
}
