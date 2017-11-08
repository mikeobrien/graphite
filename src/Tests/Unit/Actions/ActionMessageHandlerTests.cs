using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Graphite;
using Graphite.Actions;
using Graphite.Behaviors;
using Graphite.DependencyInjection;
using Graphite.Exceptions;
using Graphite.Monitoring;
using Graphite.StructureMap;
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
        private Configuration _configuration;
        private IContainer _container;
        private RequestGraph _requestGraph;
        private HttpRequestMessage _request;
        private ActionDescriptor _action;
        private HttpResponseMessage _response;
        private IBehaviorChainInvoker _invoker;
        private Metrics _metrics;
        private ActionMessageHandler _messageHandler;

        [SetUp]
        public void Setup()
        {
            _configuration = new Configuration();
            _container = new Container();
            _requestGraph = RequestGraph.Create();
            _request = _requestGraph.GetHttpRequestMessage();
            _action = _requestGraph.GetActionDescriptor();
            _response = new HttpResponseMessage();
            _metrics = new Metrics();
            _invoker = Substitute.For<IBehaviorChainInvoker>();
            _messageHandler = new ActionMessageHandler(_configuration, 
                _action, _invoker, _container, _metrics);
        }

        [Test]
        public async Task Should_call_invoker()
        {
            _invoker.Invoke(_action, _request, _requestGraph.CancellationToken).Returns(_response);

            var result = await _messageHandler.SendAsync(_request, _requestGraph.CancellationToken);

            result.ShouldEqual(_response);

            _metrics.TotalRequests.ShouldEqual(1);
            _metrics.GetAverageRequestTime(_action).ShouldBeGreaterThan(TimeSpan.Zero);
        }

        [Test]
        public async Task Should_call_unhandled_exception_handler_when_exception_is_thrown()
        {
            var exception = new Exception();
            _invoker.Invoke(_action, _request, _requestGraph.CancellationToken)
                .Throws(exception);

            var response = await _messageHandler.Should().Throw<UnhandledGraphiteException>(
                x => x.SendAsync(_request, _requestGraph.CancellationToken));

            response.ShouldBeType<UnhandledGraphiteException>();
            response.InnerException.ShouldEqual(exception);
        }

        [Test]
        public async Task Should_not_handle_unhandled_graphite_exceptions()
        {
            var exception = new UnhandledGraphiteException(null, null, new Exception());
            _invoker.Invoke(_action, _request, _requestGraph.CancellationToken)
                .Throws(exception);

            var response = await _messageHandler.Should().Throw<UnhandledGraphiteException>(
                x => x.SendAsync(_request, _requestGraph.CancellationToken));

            response.ShouldEqual(exception);
        }

        [Test]
        public async Task Should_return_400_when_bad_request_exception_caught()
        {
            var exception = new BadRequestException("fark");
            _invoker.Invoke(_action, _request, _requestGraph.CancellationToken)
                .Throws(exception);

            var response = await _messageHandler.SendAsync(_request, 
                _requestGraph.CancellationToken);

            response.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
            response.ReasonPhrase.ShouldEqual("fark");
        }
    }
}
