using System;
using System.Net;
using System.Net.Http;
using Graphite;
using Graphite.Actions;
using Graphite.DependencyInjection;
using Graphite.StructureMap;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Actions
{
    [TestFixture]
    public class UnhandledExceptionHandlerTests
    {
        private Configuration _configuration;
        private IContainer _container;
        private UnhandledExceptionHandler _exceptionHandler;

        [SetUp]
        public void Setup()
        {
            _configuration = new Configuration();
            _container = new Container();
            _exceptionHandler = new UnhandledExceptionHandler(_configuration, _container);
        }

        [Test]
        public void Should_wrap_exceptions_in_unhandled_exception()
        {
            var exception = new Exception();
            
            var unhandledException = _exceptionHandler.Should()
                .Throw<UnhandledGraphiteRequestException>(
                    x => x.HandleException(exception, null, null));
            
            unhandledException.InnerException.ShouldEqual(exception);
        }

        [Test]
        public void Should_return_exception_if_configured()
        {
            _configuration.ReturnErrorMessage = x => true;
            var exception = new Exception();
            var requestGraph = RequestGraph.Create();
            var requestMessage = new HttpRequestMessage();

            var responseMessage = _exceptionHandler.Should()
                .NotThrow(x => x.HandleException(exception, 
                    requestGraph.GetActionDescriptor(), requestMessage));

            responseMessage.StatusCode.ShouldEqual(HttpStatusCode.InternalServerError);
            var message = responseMessage.Content.ReadAsStringAsync().Result;

            message.ShouldContain(requestGraph.ActionMethod.ToString());
            message.ShouldContain(requestGraph.GetRouteDescriptor().Url);
            message.ShouldContain(requestMessage.ToString().Substring(0, 50));
            message.ShouldContain(exception.ToString().Substring(0, 50));
            message.ShouldContain(_container.GetConfiguration());
        }
    }
}
