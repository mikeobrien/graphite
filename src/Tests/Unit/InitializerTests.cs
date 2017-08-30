using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Routing;
using Graphite;
using Graphite.Actions;
using Graphite.Behaviors;
using Graphite.DependencyInjection;
using Graphite.Exceptions;
using Graphite.Extensions;
using Graphite.Reflection;
using Graphite.Routing;
using Graphite.StructureMap;
using NSubstitute;
using NUnit.Framework;
using Should;
using Tests.Common;
using Tests.Common.Fakes;

namespace Tests.Unit
{
    [TestFixture]
    public class InitializerTests
    {
        private HttpRouteCollection _routes;
        private List<IActionSource> _actionSources;
        private List<IActionDecorator> _actionDecorators;
        private Configuration _configuration;
        private IContainer _container;
        private HttpConfiguration _httpConfiguration;
        private Initializer _initializer;

        [SetUp]
        public void Setup()
        {
            _routes = new HttpRouteCollection();
            _actionSources = new List<IActionSource>();
            _actionDecorators = new List<IActionDecorator>();
            var behaviorChainInvoker = Substitute.For<IBehaviorChainInvoker>();
            _container = new Container(x =>
            {
                x.For<IBehaviorChainInvoker>().Use(behaviorChainInvoker);
            });
            _container.Register(_container);
            _configuration = new Configuration();
            _httpConfiguration = new HttpConfiguration(_routes);
            _initializer = new Initializer(new HttpRouteMapper(_container,  
                    new DefaultInlineConstraintResolver(), new List<IHttpRouteDecorator>(), 
                    _configuration, _httpConfiguration), 
                _actionSources, _container, _actionDecorators, 
                _configuration, _httpConfiguration);
        }

        public class Handler
        {
            public void Get() { }
        }

        [Test]
        public void Should_add_routes_for_actions()
        {
            AddRoute(AddActionSource(), "fark1");
            AddRoute(AddActionSource(), "fark2");

            _initializer.Initialize();

            _routes.Count.ShouldEqual(2);

            var route = _routes["GET:fark1"];
            route.Handler.ShouldNotBeNull();
            route.Handler.ShouldBeType<ActionMessageHandler>();
            route.RouteTemplate.ShouldEqual("fark1");
            var methodConstraint = route.Constraints[Graphite.Routing.Extensions
                .HttpMethodConstraintName].As<HttpMethodConstraint>();
            methodConstraint.ShouldNotBeNull();
            methodConstraint.AllowedMethods.ShouldContain(x => x.Method == "GET");

            route = _routes["GET:fark2"];
            route.Handler.ShouldNotBeNull();
            route.Handler.ShouldBeType<ActionMessageHandler>();
            route.RouteTemplate.ShouldEqual("fark2");
            methodConstraint = route.Constraints[Graphite.Routing.Extensions
                .HttpMethodConstraintName].As<HttpMethodConstraint>();
            methodConstraint.ShouldNotBeNull();
            methodConstraint.AllowedMethods.ShouldContain(x => x.Method == "GET");
        }

        [Test]
        public void Should_sort_url_parameters_below_segments()
        {
            AddRoute(AddActionSource(), "fark/{param}");
            AddRoute(AddActionSource(), "fark/segment");
            AddRoute(AddActionSource(), "fark");

            _initializer.Initialize();

            _routes.Select(x => x.RouteTemplate).ShouldOnlyContain("fark", "fark/segment", "fark/{param}");
        }
        
        [Test]
        public void Should_fail_to_add_duplicate_routes()
        {
            var actionSource = AddActionSource();
            AddRoute(actionSource, "fark");
            AddRoute(actionSource, "fark");

            var message = _initializer.Should().Throw<DuplicateRouteException>(
                x => x.Initialize()).Message;

            message.ShouldContain("fark");
            message.ShouldContain(typeof(Handler).FullName);
        }

        [Test]
        public void Should_register_runtime_configuration()
        {
            var actionSource = AddActionSource();
            AddRoute(actionSource, "fark1");
            AddRoute(actionSource, "fark2");

            _initializer.Initialize();
            
            _container.GetInstance<RuntimeConfiguration>().Actions.Count().ShouldEqual(2);
        }

        public class SomeType { }

        [TestCase(false, false, false)]
        [TestCase(true, false, false)]
        [TestCase(false, true, false)]
        [TestCase(true, true, true)]
        public void Should_run_action_decorators_that_apply(bool configAppliesTo, bool runtimeAppliesTo, bool ran)
        {
            var action = AddRoute(AddActionSource(), "fark");
            var instance = new SomeType();
            var decorator = new TestActionDecorator
            {
                AppliesToFunc = x => runtimeAppliesTo,
                DecorateFunc = x => x.ActionDescriptor.Registry.Register(instance)
            };
            _configuration.ActionDecorators.Configure(c => c.Append<TestActionDecorator>(x => configAppliesTo));
            _actionDecorators.Add(decorator);

            _initializer.Initialize();

            action.Registry.Any(x => x.PluginType == typeof(SomeType)).ShouldEqual(ran);

            decorator.AppliesToCalled.ShouldEqual(configAppliesTo);
            decorator.DecorateCalled.ShouldEqual(configAppliesTo && runtimeAppliesTo);

            if (configAppliesTo)
            {
                decorator.AppliesToContext.ActionDescriptor.ShouldEqual(action);
            }
            else decorator.AppliesToContext.ShouldBeNull();

            if (configAppliesTo && runtimeAppliesTo)
            {
                decorator.AppliesToContext.ActionDescriptor.ShouldEqual(action);
            }
            else decorator.DecorateContext.ShouldBeNull();
        }

        private List<ActionDescriptor> AddActionSource(bool appliesTo = true)
        {
            var actions = new List<ActionDescriptor>();
            var actionSource = Substitute.For<IActionSource>();
            actionSource.Applies().Returns(appliesTo);
            actionSource.GetActions().Returns(actions);
            _actionSources.Add(actionSource);
            return actions;
        }
        
        private ActionDescriptor AddRoute(List<ActionDescriptor> actions, string route)
        {
            var descriptor = new ActionDescriptor(ActionMethod.From<Handler>(x => x.Get()),
                new RouteDescriptor("GET", route, null, null, null, null), 
                null, null, null, null, null, null, new TypeCache());
            actions.Add(descriptor);
            return descriptor;
        }
    }
}
