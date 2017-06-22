using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Graphite;
using Graphite.Actions;
using Graphite.Behaviors;
using Graphite.Extensions;
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
        private Configuration _configuration;
        private RequestGraph _requestGraph;
        private HttpRequestMessage _request;
        private ActionDescriptor _descriptor;
        private HttpResponseMessage _response;
        private List<IInterceptor> _interceptors;
        private IBehaviorChainInvoker _invoker;
        private IUnhandledExceptionHandler _unhandledExceptionHandler;
        private ActionMessageHandler _messageHandler;

        [SetUp]
        public void Setup()
        {
            _configuration = new Configuration();
            _interceptors = new List<IInterceptor>();
            _requestGraph = RequestGraph.Create();
            _request = _requestGraph.GetHttpRequestMessage();
            _descriptor = _requestGraph.GetActionDescriptor();
            _response = new HttpResponseMessage();
            _invoker = Substitute.For<IBehaviorChainInvoker>();
            _unhandledExceptionHandler = Substitute.For<IUnhandledExceptionHandler>();
            _messageHandler = new ActionMessageHandler(new ConfigurationContext(_configuration, null),
                _interceptors, _descriptor, _unhandledExceptionHandler, _invoker, new Metrics());
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

        public interface ITestInterceptor1 : IInterceptor { }
        public interface ITestInterceptor2 : IInterceptor { }

        [Test]
        public async Task Should_intercept_if_interceptor_applies()
        {
            var interceptor1 = _interceptors.AddItem(Substitute.For<ITestInterceptor1>());
            var interceptor2 = _interceptors.AddItem(Substitute.For<ITestInterceptor2>());

            interceptor1.AppliesTo(null).ReturnsForAnyArgs(false);
            interceptor2.AppliesTo(null).ReturnsForAnyArgs(true);
            interceptor2.Intercept(null).ReturnsForAnyArgs(_response);

            var result = await _messageHandler.SendAsync(_request, _requestGraph.CancellationToken);

            result.ShouldEqual(_response);

            await _invoker.DidNotReceiveWithAnyArgs().Invoke(null, null, _requestGraph.CancellationToken);
        }
    }
}
