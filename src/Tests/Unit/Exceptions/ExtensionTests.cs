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
    public class ExtensionTests
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
            _urlTemplate = "http://www.fark.com/{fark}";
            _actionDescriptor = new ActionDescriptor(_actionMethod,
                new RouteDescriptor("POST", _urlTemplate, null, null, null, null),
                null, null, null, null, null, null, null, null);
        }

        [Test]
        public void Should_pretty_print_exception()
        {
            var container = new TrackingContainer(new Container(), new TypeCache());
            container.Register(new object());
            var exception = GenerateException();

            var response = exception.GetDebugResponse(_requestMessage);
            
            response.ShouldContain(_requestMessage.RawHeaders());
            response.ShouldContain(_requestMessage.RequestUri.ToString());
            response.ShouldContain(exception.Message);
            response.ShouldContainAll(exception.StackTrace.Split(" in ").Trim());
        }

        [Test]
        public void Should_pretty_print_unhandled_graphite_exception()
        {
            var container = new TrackingContainer(new Container(), new TypeCache());
            container.Register(new object());
            var exception = GenerateUnhandledGraphiteException(container);

            var response = exception.GetDebugResponse(_requestMessage);

            response.ShouldContain(_actionMethod.FullName);
            response.ShouldContain(_urlTemplate);
            response.ShouldContain(_requestMessage.RawHeaders());
            response.ShouldContain(_requestMessage.RequestUri.ToString());
            response.ShouldContain(exception.InnerException.Message);
            response.ShouldContainAll(exception.InnerException.StackTrace.Split(" in ").Trim());
            response.ShouldContain(container.GetConfiguration());
            response.ShouldContain(container.Registry.ToString());
        }

        [Test]
        public void Should_pretty_print_unhandled_graphite_exception_and_include_root_container()
        {
            var rootContainer = new TrackingContainer(new Container(), new TypeCache());
            var requestContainer = new TrackingContainer(new Container(), new TypeCache(), rootContainer);
            rootContainer.Register(new object());
            requestContainer.Register(new object());

            var exception = GenerateUnhandledGraphiteException(requestContainer);
            var response = exception.GetDebugResponse(_requestMessage);

            response.ShouldContain(_actionMethod.FullName);
            response.ShouldContain(_urlTemplate);
            response.ShouldContain(_requestMessage.RawHeaders());
            response.ShouldContain(_requestMessage.RequestUri.ToString());
            response.ShouldContain(exception.InnerException.Message);
            response.ShouldContainAll(exception.InnerException.StackTrace.Split(" in ").Trim());
            response.ShouldContain(rootContainer.GetConfiguration());
            response.ShouldContain(rootContainer.Registry.ToString());
            response.ShouldContain(requestContainer.GetConfiguration());
            response.ShouldContain(requestContainer.Registry.ToString());
        }

        private Exception GenerateUnhandledGraphiteException(IContainer requestContainer)
        {
            try
            {
                try
                {
                    throw new Exception("fark");
                }
                catch (Exception e)
                {
                    throw new UnhandledGraphiteException(_actionDescriptor, requestContainer, e);
                }
            }
            catch (Exception e)
            {
                return e;
            }
        }

        private Exception GenerateException()
        {
            try
            {
                throw new Exception("fark");
            }
            catch (Exception e)
            {
                return e;
            }
        }

        [Test]
        public void Should_friendly_format_exception()
        {
            Exception exception = null;
            try
            {
                ExceptionLevel1();
            }
            catch (Exception e)
            {
                exception = e;
            }

            var message = exception.ToFriendlyException();

            Console.WriteLine(message);

            message.ShouldContain(exception.GetType().FullName);
            message.ShouldContain(exception.Message);
            message.ShouldContain(exception.InnerException.Message);
            message.ShouldContain(exception.InnerException.InnerException.Message);
        }

        private void ExceptionLevel1()
        {
            try
            {
                ExceptionLevel2();
            }
            catch (Exception e)
            {
                throw new Exception("level 1", e);
            }
        }

        private void ExceptionLevel2()
        {
            try
            {
                ExceptionLevel3();
            }
            catch (Exception e)
            {
                throw new Exception("level 2", e);
            }
        }
        private void ExceptionLevel3() => throw new Exception("level 3");
    }
}
