using System;
using System.Net;
using System.Net.Http;
using Graphite;
using Graphite.Actions;
using Graphite.DependencyInjection;
using Graphite.Exceptions;
using Graphite.Http;
using NSubstitute;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Exceptions
{
    [TestFixture]
    public class ExceptionHandlerTests
    {
        private Configuration _configuration;
        private IExceptionDebugResponse _debugResponse;
        private ActionDescriptor _actionDescriptor;
        private HttpRequestMessage _requestMessage;
        private IContainer _container;
        private ExceptionHandler _exceptionHandler;

        [SetUp]
        public void Setup()
        {
            _configuration = new Configuration();
            _debugResponse = Substitute.For<IExceptionDebugResponse>();
            _container = Substitute.For<IContainer>();
            _actionDescriptor = new ActionDescriptor(null, null,
                null, null, null, null, null, null, null);
            _requestMessage = new HttpRequestMessage();
            _exceptionHandler = new ExceptionHandler(_configuration, _debugResponse);
        }

        [Test]
        public void Should_wrap_exceptions_in_unhandled_exceptions()
        {
            var exception = new Exception();

            var result = _exceptionHandler.Should().Throw<UnhandledGraphiteException>(x => x
                .HandleException(exception, _actionDescriptor,
                    _requestMessage, _container));

            result.InnerException.ShouldEqual(exception);
        }

        [Test]
        public void Should_return_error_debug_message_if_validation_exception_thrown()
        {
            _debugResponse.GetResponse(null).ReturnsForAnyArgs("fark");
            _configuration.ReturnErrorMessage = x => true;
            var exception = new Exception();

            var result = _exceptionHandler.HandleException(exception,
                _actionDescriptor, _requestMessage, _container);

            result.StatusCode.ShouldEqual(HttpStatusCode.InternalServerError);
            result.ReasonPhrase.ShouldEqual(_configuration.UnhandledExceptionStatusText);
            result.Content.Headers.ContentType.MediaType.ShouldEqual(MimeTypes.TextPlain);
            result.Content.ReadAsStringAsync().Result.ShouldEqual("fark");
        }
    }
}
