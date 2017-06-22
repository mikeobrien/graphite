using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Routing;
using System.Web.Http.Routing.Constraints;
using Graphite;
using Graphite.Actions;
using Graphite.Behaviors;
using Graphite.DependencyInjection;
using Graphite.Extensions;
using Graphite.Routing;
using NSubstitute;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Routing
{
    [TestFixture]
    public class HttpRouteMapperTests
    {
        public class Handler
        {
            public void Get_Param([NUnit.Framework.Range(4, 8)] int param) { }
        }

        private List<IHttpRouteDecorator> _routeDecorators;

        [SetUp]
        public void Setup()
        {
            _routeDecorators = new List<IHttpRouteDecorator>();
        }

        [Test]
        public void Should_set_handler()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(x => x.Get_Param(0))
                .WithUrl("fark")
                .ConfigureContainer(x => x.Register(Substitute.For<IUnhandledExceptionHandler>()));

            Map(requestGraph);

            var handler = requestGraph.HttpConfiguration.Routes.First().Handler;
            handler.ShouldNotBeNull();
            handler.ShouldBeType<ActionMessageHandler>();
        }

        [Test]
        public void Should_detokenize_url()
        {
            var requestGraph = RequestGraph.Create()
                .WithUrl("fark/{param:range(4, 8)}/farker")
                .ConfigureContainer(x => x.Register(Substitute.For<IUnhandledExceptionHandler>()));

            Map(requestGraph);

            requestGraph.HttpConfiguration.Routes.First().RouteTemplate
                .ShouldEqual("fark/{param}/farker");
        }

        [Test]
        public void Should_add_method_constraint()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(x => x.Get_Param(0))
                .WithUrl("fark")
                .ConfigureContainer(x => x.Register(Substitute.For<IUnhandledExceptionHandler>()));

            Map(requestGraph);

            var constraint = requestGraph.HttpConfiguration.Routes.First().Constraints[Graphite.Routing
                .Extensions.HttpMethodConstraintName] as HttpMethodConstraint;

            constraint.ShouldNotBeNull();
            constraint.AllowedMethods.ShouldContain(x => x.Method == "GET");
        }

        [Test]
        public void Should_add_parameter_constraints()
        {
            var requestGraph = RequestGraph.Create()
                .WithUrl("fark/{param:range(4, 8)}/farker")
                .ConfigureContainer(x => x.Register(Substitute.For<IUnhandledExceptionHandler>()));

            Map(requestGraph);

            var constraint = requestGraph.HttpConfiguration.Routes.First()
                .Constraints["param"] as RangeRouteConstraint;
            constraint.ShouldNotBeNull();
            constraint.Min.ShouldEqual(4);
            constraint.Max.ShouldEqual(8);
        }

        public interface IRouteDecorator1 : IHttpRouteDecorator { }
        public interface IRouteDecorator2 : IHttpRouteDecorator { }
        public interface IRouteDecorator3 : IHttpRouteDecorator { }
        public interface IRouteDecorator4 : IHttpRouteDecorator { }

        [Test]
        public void Should_decorate_route_with_decorators_that_apply()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(x => x.Get_Param(0))
                .WithUrl("fark")
                .Configure(x => x.ConfigureHttpRouteDecorators(d => d
                    .Append<TestHttpRouteDecorator1>()
                    .Append<TestHttpRouteDecorator2>(c => false)
                    .Append<TestHttpRouteDecorator3>()
                    .Append<TestHttpRouteDecorator4>()))
                .ConfigureContainer(x => x.Register(Substitute.For<IUnhandledExceptionHandler>()));

            var decorator1 = new TestHttpRouteDecorator1();
            var decorator2 = new TestHttpRouteDecorator2 { AppliesToReturns = true };
            var decorator3 = new TestHttpRouteDecorator3 { AppliesToReturns = true };
            var decorator4 = new TestHttpRouteDecorator4 { AppliesToReturns = true };

            _routeDecorators.AddItem(decorator4);
            _routeDecorators.AddItem(decorator3);
            _routeDecorators.AddItem(decorator2);
            _routeDecorators.AddItem(decorator1);

            Map(requestGraph);

            decorator1.AppliesToWasCalled.ShouldBeTrue();
            decorator1.DecorateWasCalled.ShouldBeFalse();

            decorator2.AppliesToWasCalled.ShouldBeFalse();
            decorator2.DecorateWasCalled.ShouldBeFalse();

            decorator3.AppliesToWasCalled.ShouldBeTrue();
            decorator3.DecorateWasCalled.ShouldBeTrue();

            decorator4.AppliesToWasCalled.ShouldBeTrue();
            decorator4.DecorateWasCalled.ShouldBeTrue();
        }

        private void Map(RequestGraph requestGraph)
        {
            requestGraph.Container.Register(Substitute.For<IBehaviorChainInvoker>());
            new HttpRouteMapper(requestGraph.HttpConfiguration, 
                    requestGraph.Container, new DefaultInlineConstraintResolver(), 
                    _routeDecorators, new ConfigurationContext(requestGraph.Configuration, null))
                .Map(requestGraph.GetActionDescriptor());
        }
    }

    public class TestHttpRouteDecorator1 : TestHttpRouteDecoratorBase { }
    public class TestHttpRouteDecorator2 : TestHttpRouteDecoratorBase { }
    public class TestHttpRouteDecorator3 : TestHttpRouteDecoratorBase { }
    public class TestHttpRouteDecorator4 : TestHttpRouteDecoratorBase { }

    public abstract class TestHttpRouteDecoratorBase : IHttpRouteDecorator
    {
        public bool AppliesToReturns { get; set; }
        public bool AppliesToWasCalled { get; private set; }
        public bool DecorateWasCalled { get; private set; }

        public bool AppliesTo(HttpRouteDecoratorContext context)
        {
            AppliesToWasCalled = true;
            return AppliesToReturns;
        }

        public void Decorate(HttpRouteDecoratorContext route)
        {
            DecorateWasCalled = true;
        }
    }
}
