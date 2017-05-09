using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Routing;
using Graphite;
using Graphite.Actions;
using Graphite.DependencyInjection;
using Graphite.Extensions;
using Graphite.Monitoring;
using Graphite.Routing;
using Graphite.StructureMap;
using NSubstitute;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit
{
    [TestFixture]
    public class InitializerTests
    {
        private HttpRouteCollection _routes;
        private List<IActionSource> _actionSources;
        private Configuration _configuration;
        private IContainer _container;
        private IBehaviorChainInvoker _invoker;
        private HttpConfiguration _httpConfiguration;
        private Initializer _initializer;

        [SetUp]
        public void Setup()
        {
            _routes = new HttpRouteCollection();
            _actionSources = new List<IActionSource>();
            _invoker = Substitute.For<IBehaviorChainInvoker>();
            _container = new Container();
            _configuration = new Configuration();
            _httpConfiguration = new HttpConfiguration(_routes);
            _initializer = new Initializer(_actionSources, _invoker, 
                _container, _configuration, new Metrics());
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

            _initializer.Initialize(_httpConfiguration);

            _routes.Count.ShouldEqual(2);

            var route = _routes["GET:fark1"];
            route.Handler.ShouldNotBeNull();
            route.Handler.ShouldBeType<ActionMessageHandler>();
            route.RouteTemplate.ShouldEqual("fark1");
            var methodConstraint = route.Constraints["httpMethod"].As<HttpMethodConstraint>();
            methodConstraint.ShouldNotBeNull();
            methodConstraint.AllowedMethods.ShouldContain(x => x.Method == "GET");

            route = _routes["GET:fark2"];
            route.Handler.ShouldNotBeNull();
            route.Handler.ShouldBeType<ActionMessageHandler>();
            route.RouteTemplate.ShouldEqual("fark2");
            methodConstraint = route.Constraints["httpMethod"].As<HttpMethodConstraint>();
            methodConstraint.ShouldNotBeNull();
            methodConstraint.AllowedMethods.ShouldContain(x => x.Method == "GET");
        }

        [Test]
        public void Should_not_add_routes_for_action_sources_that_do_not_apply()
        {
            AddRoute(AddActionSource(false), "fark1");
            AddRoute(AddActionSource(true), "fark2");

            _initializer.Initialize(_httpConfiguration);
            
            _routes.Count.ShouldEqual(1);
            _routes.First().RouteTemplate.ShouldEqual("fark2");
        }

        [Test]
        public void Should_fail_to_add_duplicate_routes()
        {
            var actionSource = AddActionSource();
            AddRoute(actionSource, "fark");
            AddRoute(actionSource, "fark");

            var message = _initializer.Should().Throw<DuplicateRouteException>(
                x => x.Initialize(_httpConfiguration)).Message;

            message.ShouldContain("fark");
            message.ShouldContain(typeof(Handler).FullName);
        }

        [Test]
        public void Should_register_configuration()
        {
            var actionSource = AddActionSource();
            AddRoute(actionSource, "fark1");
            AddRoute(actionSource, "fark2");

            _initializer.Initialize(_httpConfiguration);

            _container.GetInstance<HttpConfiguration>().ShouldEqual(_httpConfiguration);
            _container.GetInstance<RuntimeConfiguration>().Actions.Count().ShouldEqual(2);
        }

        private List<ActionDescriptor> AddActionSource(bool appliesTo = true)
        {
            var actions = new List<ActionDescriptor>();
            var actionSource = Substitute.For<IActionSource>();
            actionSource.AppliesTo(Arg.Any<ActionSourceContext>()).Returns(appliesTo);
            actionSource.GetActions(Arg.Any<ActionSourceContext>()).Returns(actions);
            _actionSources.Add(actionSource);
            return actions;
        }

        private void AddRoute(List<ActionDescriptor> actions, string route)
        {
            actions.Add(new ActionDescriptor(Type<Handler>.Expression(x => x.Get()).ToActionMethod(),
                new RouteDescriptor("GET", route, null, null, null, null, null)));
        }
    }
}
