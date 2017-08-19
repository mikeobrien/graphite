using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Graphite.Actions;
using Graphite.DependencyInjection;
using Graphite.Exceptions;
using Graphite.Extensions;
using Graphite.Http;
using Graphite.Reflection;
using Graphite.Routing;
using Graphite.StructureMap;
using NUnit.Framework;
using Should;
using Tests.Common;
using HttpMethod = System.Net.Http.HttpMethod;

namespace Tests.Unit.Exceptions
{
    [TestFixture]
    public class ExceptionDebugResponseTests
    {
        public class Handler { public void Post() { } }

        private ActionDescriptor _actionDescriptor;
        private HttpRequestMessage _requestMessage;
        private string _urlTemplate;
        private ActionMethod _actionMethod;

        [SetUp]
        public void Setup()
        {
            _requestMessage = new HttpRequestMessage(HttpMethod.Post, "http://www.fark.com");
            _requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MimeTypes.ApplicationAtomXml));
            _actionMethod = ActionMethod.From<Handler>(x => x.Post());
            _urlTemplate ="http://www.fark.com/{fark}" ;
            _actionDescriptor = new ActionDescriptor(_actionMethod, 
                new RouteDescriptor("POST", _urlTemplate, null, null, null, null), 
                null, null, null, null, null, null, null);
        }

        [Test]
        public void Should_pretty_print_exception()
        {
            Exception exception;
            try
            {
                throw new Exception("fark");
            }
            catch (Exception e)
            {
                exception = e;
            }
            var container = new TrackingContainer(new Container(), new TypeCache());
            container.Register(new object());

            var response = new ExceptionDebugResponse()
                .GetResponse(exception, _actionDescriptor, _requestMessage, container);

            response.ShouldContain(_actionMethod.FullName);
            response.ShouldContain(_urlTemplate);
            response.ShouldContain(_requestMessage.RawHeaders());
            response.ShouldContain(_requestMessage.RequestUri.ToString());
            response.ShouldContain(exception.Message);
            response.ShouldContainAll(exception.StackTrace.Split(" in ").Trim());
            response.ShouldContain(container.GetConfiguration());
            response.ShouldContain(container.Registry.ToString());
        }

        [Test]
        public void Should_pretty_print_exception_and_include_root_container()
        {
            Exception exception;
            try
            {
                throw new Exception("fark");
            }
            catch (Exception e)
            {
                exception = e;
            }
            var rootContainer = new TrackingContainer(new Container(), new TypeCache());
            var requestContainer = new TrackingContainer(new Container(), new TypeCache(), rootContainer);
            rootContainer.Register(new object());
            requestContainer.Register(new object());

            var response = new ExceptionDebugResponse()
                .GetResponse(exception, _actionDescriptor, _requestMessage, requestContainer);
            
            response.ShouldContain(_actionMethod.FullName);
            response.ShouldContain(_urlTemplate);
            response.ShouldContain(_requestMessage.RawHeaders());
            response.ShouldContain(_requestMessage.RequestUri.ToString());
            response.ShouldContain(exception.Message);
            response.ShouldContainAll(exception.StackTrace.Split(" in ").Trim());
            response.ShouldContain(rootContainer.GetConfiguration());
            response.ShouldContain(rootContainer.Registry.ToString());
            response.ShouldContain(requestContainer.GetConfiguration());
            response.ShouldContain(requestContainer.Registry.ToString());
        }
    }
}
