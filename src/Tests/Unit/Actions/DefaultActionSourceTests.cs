using System.Collections.Generic;
using System.Linq;
using Graphite;
using Graphite.Actions;
using Graphite.Behaviors;
using Graphite.Reflection;
using Graphite.Routing;
using NUnit.Framework;
using Should;
using Tests.Common;
using Tests.Common.Fakes;
using Tests.Unit.Actions.ActionSourceTests;

namespace Tests.Unit.Actions
{
    namespace ActionSourceTests
    {
        public class Handler1
        {
            public object Get() { return null; }
            public object Post() { return null; }
        }

        public class Handler2
        {
            public object Get() { return null; }
            public object Post() { return null; }
        }
    }

    [TestFixture]
    public class DefaultActionSourceTests
    {
        private ITypeCache _typeCache;
        private Configuration _configuration;
        private TestActionMethodSource _actionMethodSource1;
        private TestActionMethodSource _actionMethodSource2;
        private TestRouteConvention _routeConvention1;
        private TestRouteConvention _routeConvention2;
        private ActionMethod _handler1Get;
        private ActionMethod _handler1Post;
        private ActionMethod _handler2Get;
        private ActionMethod _handler2Post;
        private DefaultActionSource _actionSource;

        public class TestRouteConvention1 : TestRouteConvention { }
        public class TestRouteConvention2 : TestRouteConvention { }

        public class TestActionMethodSource1 : TestActionMethodSource { }
        public class TestActionMethodSource2 : TestActionMethodSource { }

        [SetUp]
        public void Setup()
        {
            _typeCache = new TypeCache();
            _configuration = new Configuration();
            _actionMethodSource1 = new TestActionMethodSource1();
            _actionMethodSource2 = new TestActionMethodSource2();
            _routeConvention1 = new TestRouteConvention1
            {
                GetDescriptors = x => new List<RouteDescriptor>
                {
                    new RouteDescriptor(x.ActionMethod.MethodDescriptor.Name, 
                        x.ActionMethod.FullName, null, null, null, null)
                }
            };
            _routeConvention2 = new TestRouteConvention2
            {
                GetDescriptors = _routeConvention1.GetDescriptors
            };

            _handler1Get = _actionMethodSource1.Add<Handler1>(x => x.Get());
            _handler1Post = _actionMethodSource1.Add<Handler1>(x => x.Post());
            _handler2Get = _actionMethodSource2.Add<Handler2>(x => x.Get());
            _handler2Post = _actionMethodSource2.Add<Handler2>(x => x.Post());

            _routeConvention1.AppliesToPredicate = x => true;
            _routeConvention2.AppliesToPredicate = x => true;

            _actionSource = new DefaultActionSource(
                new ConfigurationContext(_configuration, null), 
                new List<IActionMethodSource>
                {
                    _actionMethodSource1, _actionMethodSource2
                }, 
                new List<IRouteConvention>
                {
                    _routeConvention1, _routeConvention2
                },
                new TypeCache());
        }

        [Test]
        public void Should_conditionally_query_action_method_sources_from_configuration()
        {
            _configuration.ActionMethodSources.Append<TestActionMethodSource1>(x => false);

            var actions = _actionSource.GetActions();

            actions.Count.ShouldEqual(4);

            _actionMethodSource1.AppliesCalled.ShouldBeFalse();
            _actionMethodSource1.GetActionMethodsCalled.ShouldBeFalse();

            _actionMethodSource2.AppliesCalled.ShouldBeTrue();
            _actionMethodSource2.GetActionMethodsCalled.ShouldBeTrue();
        }

        [Test]
        public void Should_conditionally_query_action_method_sources_from_instance()
        {
            _actionMethodSource1.AppliesFunc = () => false;

            var actions = _actionSource.GetActions();

            actions.Count.ShouldEqual(4);

            _actionMethodSource1.AppliesCalled.ShouldBeTrue();
            _actionMethodSource1.GetActionMethodsCalled.ShouldBeFalse();

            _actionMethodSource2.AppliesCalled.ShouldBeTrue();
            _actionMethodSource2.GetActionMethodsCalled.ShouldBeTrue();
        }

        [Test]
        public void Should_conditionally_apply_route_conventions_by_configuration()
        {
            _configuration.RouteConventions.Append<TestRouteConvention1>(
                x => x.ActionMethod.HandlerTypeDescriptor.Type == typeof(Handler1));
            _configuration.RouteConventions.Append<TestRouteConvention2>(
                x => x.ActionMethod.HandlerTypeDescriptor.Type == typeof(Handler2) &&
                    x.ActionMethod.MethodDescriptor.Name == nameof(Handler2.Post));

            var actions = _actionSource.GetActions();

            actions.Count.ShouldEqual(3);

            var action = actions.FirstOrDefault(x => x.Action == _handler1Get);
            action.ShouldNotBeNull();
            action.Route.Method.ShouldEqual(_handler1Get.MethodDescriptor.Name);
            action.Route.Url.ShouldEqual(_handler1Get.FullName);

            action = actions.FirstOrDefault(x => x.Action == _handler1Post);
            action.ShouldNotBeNull();
            action.Route.Method.ShouldEqual(_handler1Post.MethodDescriptor.Name);
            action.Route.Url.ShouldEqual(_handler1Post.FullName);

            action = actions.FirstOrDefault(x => x.Action == _handler2Post);
            action.ShouldNotBeNull();
            action.Route.Method.ShouldEqual(_handler2Post.MethodDescriptor.Name);
            action.Route.Url.ShouldEqual(_handler2Post.FullName);

            _routeConvention1.AppliesToCalled.ShouldBeTrue();
            _routeConvention1.GetRouteDescriptorsCalled.ShouldBeTrue();
            _routeConvention1.GetRouteDescriptorsContext.ActionMethod.ShouldNotBeNull();

            _routeConvention2.AppliesToCalled.ShouldBeTrue();
            _routeConvention2.GetRouteDescriptorsCalled.ShouldBeTrue();
            _routeConvention2.GetRouteDescriptorsContext.ActionMethod.ShouldNotBeNull();
        }

        [Test]
        public void Should_conditionally_apply_route_conventions_by_instance()
        {
            _routeConvention1.AppliesToPredicate = x => x.ActionMethod
                .HandlerTypeDescriptor.Type == typeof(Handler1);
            _routeConvention2.AppliesToPredicate = x => x.ActionMethod
                    .HandlerTypeDescriptor.Type == typeof(Handler2) && 
                x.ActionMethod.MethodDescriptor.Name == nameof(Handler2.Post);

            var actions = _actionSource.GetActions();

            actions.Count.ShouldEqual(3);

            var action = actions.FirstOrDefault(x => x.Action == _handler1Get);
            action.ShouldNotBeNull();
            action.Route.Method.ShouldEqual(_handler1Get.MethodDescriptor.Name);
            action.Route.Url.ShouldEqual(_handler1Get.FullName);

            action = actions.FirstOrDefault(x => x.Action == _handler1Post);
            action.ShouldNotBeNull();
            action.Route.Method.ShouldEqual(_handler1Post.MethodDescriptor.Name);
            action.Route.Url.ShouldEqual(_handler1Post.FullName);

            action = actions.FirstOrDefault(x => x.Action == _handler2Post);
            action.ShouldNotBeNull();
            action.Route.Method.ShouldEqual(_handler2Post.MethodDescriptor.Name);
            action.Route.Url.ShouldEqual(_handler2Post.FullName);

            _routeConvention1.AppliesToCalled.ShouldBeTrue();
            _routeConvention1.GetRouteDescriptorsCalled.ShouldBeTrue();
            _routeConvention1.GetRouteDescriptorsContext.ActionMethod.ShouldNotBeNull();

            _routeConvention2.AppliesToCalled.ShouldBeTrue();
            _routeConvention2.GetRouteDescriptorsCalled.ShouldBeTrue();
            _routeConvention2.GetRouteDescriptorsContext.ActionMethod.ShouldNotBeNull();
        }

        [Test]
        public void Should_return_multiple_route_descriptors()
        {
            _routeConvention1.GetDescriptors = x => new List<RouteDescriptor>
                {
                    new RouteDescriptor(x.ActionMethod.MethodDescriptor.Name, 
                        x.ActionMethod.FullName, null, null, null, null),
                    new RouteDescriptor(x.ActionMethod.MethodDescriptor.Name, 
                        "fark", null, null, null, null)
                };
            _routeConvention2.GetDescriptors = x => new List<RouteDescriptor>
                {
                    new RouteDescriptor(x.ActionMethod.MethodDescriptor.Name, 
                        x.ActionMethod.FullName, null, null, null, null),
                    new RouteDescriptor(x.ActionMethod.MethodDescriptor.Name, 
                        "farker", null, null, null, null)
                };

            var actions = _actionSource.GetActions();

            actions.Count.ShouldEqual(16);

            actions.ShouldContain(x => x.Action == _handler1Get && x.Route.Url == _handler1Get.FullName);
            actions.ShouldContain(x => x.Action == _handler1Post && x.Route.Url == _handler1Post.FullName);
            actions.ShouldContain(x => x.Action == _handler2Get && x.Route.Url == _handler2Get.FullName);
            actions.ShouldContain(x => x.Action == _handler2Post && x.Route.Url == _handler2Post.FullName);

            actions.ShouldContain(x => x.Action == _handler1Get && x.Route.Url == "fark");
            actions.ShouldContain(x => x.Action == _handler1Post && x.Route.Url == "fark");
            actions.ShouldContain(x => x.Action == _handler2Get && x.Route.Url == "fark");
            actions.ShouldContain(x => x.Action == _handler2Post && x.Route.Url == "fark");

            actions.ShouldContain(x => x.Action == _handler1Get && x.Route.Url == "farker");
            actions.ShouldContain(x => x.Action == _handler1Post && x.Route.Url == "farker");
            actions.ShouldContain(x => x.Action == _handler2Get && x.Route.Url == "farker");
            actions.ShouldContain(x => x.Action == _handler2Post && x.Route.Url == "farker");
        }

        [Test]
        public void Should_return_distinct_actions()
        {
            _actionMethodSource1.Add<Handler2>(x => x.Get());

            var actions = _actionSource.GetActions();

            actions.Count.ShouldEqual(8);
        }

        [Test]
        public void Should_only_configure_matching_behaviors_on_action()
        {
            _configuration.Behaviors.Append<TestBehavior1>(x => x.ActionMethod
                .MethodDescriptor.Name == nameof(Handler1.Get));

            _configuration.Behaviors.Append<TestBehavior2>();

            var actions = _actionSource.GetActions();

            var behaviors = actions.FirstOrDefault(x => x.Action == _handler1Get).Behaviors;
            behaviors.Count().ShouldEqual(3);
            behaviors.ShouldOnlyContain(
                _typeCache.GetTypeDescriptor<DefaultErrorHandlerBehavior>(),
                _typeCache.GetTypeDescriptor<TestBehavior1>(),
                _typeCache.GetTypeDescriptor<TestBehavior2>());

            behaviors = actions.FirstOrDefault(x => x.Action == _handler1Post).Behaviors;
            behaviors.Count().ShouldEqual(2);
            behaviors.ShouldOnlyContain(
                _typeCache.GetTypeDescriptor<DefaultErrorHandlerBehavior>(),
                _typeCache.GetTypeDescriptor<TestBehavior2>());
        }
    }
}
