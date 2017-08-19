using System.Linq;
using Graphite.Actions;
using Graphite.Authentication;
using Graphite.Behaviors;
using Graphite.Binding;
using Graphite.Extensibility;
using Graphite.Readers;
using Graphite.Reflection;
using Graphite.Writers;
using NUnit.Framework;
using Should;
using Tests.Common;
using Tests.Common.Fakes;

namespace Tests.Unit.Actions
{
    [TestFixture]
    public class ActionDescriptorFactoryTests
    {
        private ActionDescriptorFactory _actionDescriptorFactory;
        private RequestGraph _requestGraph;

        [SetUp]
        public void Setup()
        {
            _requestGraph = RequestGraph.Create();
            
            _actionDescriptorFactory = new ActionDescriptorFactory(
                _requestGraph.Configuration, _requestGraph.HttpConfiguration, 
                new TypeCache());
        }

        [Test]
        public void Should_set_properties()
        {
            var route = _requestGraph.GetRouteDescriptor();
            var descriptor = _actionDescriptorFactory
                .CreateDescriptor(_requestGraph.ActionMethod, route);

            descriptor.Action.ShouldEqual(_requestGraph.ActionMethod);
            descriptor.Route.ShouldEqual(route);
        }

        [Test]
        public void Should_only_configure_matching_authenticators_on_action(
            [Values(true, false)] bool applies)
        {
            _requestGraph.Configuration
                .Authenticators.Configure(c => c.Clear()
                    .Append<TestAutenticator1>(x => applies)
                    .Append<TestAutenticator2>());

            var descriptor = _actionDescriptorFactory
                .CreateDescriptor(_requestGraph.ActionMethod, 
                    _requestGraph.GetRouteDescriptor());

            var authenticators = descriptor.Authenticators;

            if (applies)
            {
                authenticators.Count().ShouldEqual(2);
                authenticators.ShouldOnlyContain(
                    Plugin<IAuthenticator>.Create<TestAutenticator1>(),
                    Plugin<IAuthenticator>.Create<TestAutenticator2>());
            }
            else
            {
                authenticators.Count().ShouldEqual(1);
                authenticators.ShouldOnlyContain(
                    Plugin<IAuthenticator>.Create<TestAutenticator2>());
            }
        }

        [Test]
        public void Should_only_configure_matching_request_binders_on_action(
            [Values(true, false)] bool applies)
        {
            _requestGraph.Configuration
                .RequestBinders.Configure(c => c.Clear()
                    .Append<TestRequestBinder1>(x => applies)
                    .Append<TestRequestBinder2>());

            var descriptor = _actionDescriptorFactory
                .CreateDescriptor(_requestGraph.ActionMethod,
                    _requestGraph.GetRouteDescriptor());

            var binders = descriptor.RequestBinders;

            if (applies)
            {
                binders.Count().ShouldEqual(2);
                binders.ShouldOnlyContain(
                    Plugin<IRequestBinder>.Create<TestRequestBinder1>(),
                    Plugin<IRequestBinder>.Create<TestRequestBinder2>());
            }
            else
            {
                binders = descriptor.RequestBinders;
                binders.Count().ShouldEqual(1);
                binders.ShouldOnlyContain(
                    Plugin<IRequestBinder>.Create<TestRequestBinder2>());
            }
        }

        [Test]
        public void Should_only_configure_matching_request_readers_on_action(
            [Values(true, false)] bool applies)
        {
            _requestGraph.Configuration
                .RequestReaders.Configure(c => c.Clear()
                    .Append<TestRequestReader1>(x => applies)
                    .Append<TestRequestReader2>());

            var descriptor = _actionDescriptorFactory
                .CreateDescriptor(_requestGraph.ActionMethod,
                    _requestGraph.GetRouteDescriptor());

            var readers = descriptor.RequestReaders;

            if (applies)
            {
                readers.Count().ShouldEqual(2);
                readers.ShouldOnlyContain(
                    Plugin<IRequestReader>.Create<TestRequestReader1>(),
                    Plugin<IRequestReader>.Create<TestRequestReader2>());
            }
            else
            {
                readers = descriptor.RequestReaders;
                readers.Count().ShouldEqual(1);
                readers.ShouldOnlyContain(
                    Plugin<IRequestReader>.Create<TestRequestReader2>());
            }
        }

        [Test]
        public void Should_only_configure_matching_response_writers_on_action(
            [Values(true, false)] bool applies)
        {
            _requestGraph.Configuration
                .ResponseWriters.Configure(c => c.Clear()
                    .Append<TestResponseWriter1>(x => applies)
                    .Append<TestResponseWriter2>());

            var descriptor = _actionDescriptorFactory
                .CreateDescriptor(_requestGraph.ActionMethod,
                    _requestGraph.GetRouteDescriptor());

            var writers = descriptor.ResponseWriters;

            if (applies)
            {
                writers.Count().ShouldEqual(2);
                writers.ShouldOnlyContain(
                    Plugin<IResponseWriter>.Create<TestResponseWriter1>(),
                    Plugin<IResponseWriter>.Create<TestResponseWriter2>());
            }
            else
            {
                writers = descriptor.ResponseWriters;
                writers.Count().ShouldEqual(1);
                writers.ShouldOnlyContain(
                    Plugin<IResponseWriter>.Create<TestResponseWriter2>());
            }
        }

        [Test]
        public void Should_only_configure_matching_response_statuses_on_action(
            [Values(true, false)] bool applies)
        {
            _requestGraph.Configuration
                .ResponseStatus.Configure(c => c.Clear()
                    .Append<TestResponseStatus1>(x => applies)
                    .Append<TestResponseStatus2>());

            var descriptor = _actionDescriptorFactory
                .CreateDescriptor(_requestGraph.ActionMethod,
                    _requestGraph.GetRouteDescriptor());

            var statuses = descriptor.ResponseStatus;

            if (applies)
            {
                statuses.Count().ShouldEqual(2);
                statuses.ShouldOnlyContain(
                    Plugin<IResponseStatus>.Create<TestResponseStatus1>(),
                    Plugin<IResponseStatus>.Create<TestResponseStatus2>());
            }
            else
            {
                statuses = descriptor.ResponseStatus;
                statuses.Count().ShouldEqual(1);
                statuses.ShouldOnlyContain(
                    Plugin<IResponseStatus>.Create<TestResponseStatus2>());
            }
        }

        [Test]
        public void Should_only_configure_matching_behaviors_on_action(
            [Values(true, false)] bool applies)
        {
            _requestGraph.Configuration
                .Behaviors.Configure(c => c.Clear()
                    .Append<TestBehavior1>(x => applies)
                    .Append<TestBehavior2>());

            var descriptor = _actionDescriptorFactory
                .CreateDescriptor(_requestGraph.ActionMethod,
                    _requestGraph.GetRouteDescriptor());

            var behaviors = descriptor.Behaviors;

            if (applies)
            {
                behaviors.Count().ShouldEqual(2);
                behaviors.ShouldOnlyContain(
                    Plugin<IBehavior>.Create<TestBehavior1>(),
                    Plugin<IBehavior>.Create<TestBehavior2>());
            }
            else
            {
                behaviors = descriptor.Behaviors;
                behaviors.Count().ShouldEqual(1);
                behaviors.ShouldOnlyContain(
                    Plugin<IBehavior>.Create<TestBehavior2>());
            }
        }
    }
}
