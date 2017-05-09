using System.Linq;
using System.Web.Http.Routing;
using System.Web.Http.Routing.Constraints;
using Graphite.Actions;
using Graphite.Behaviors;
using Graphite.DependencyInjection;
using Graphite.Routing;
using NSubstitute;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Routing
{
    [TestFixture]
    public class RouteMapperTests
    {
        public class Handler
        {
            public void Get_Param([NUnit.Framework.Range(4, 8)] int param) { }
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

        private void Map(RequestGraph requestGraph)
        {
            requestGraph.Container.Register(Substitute.For<IBehaviorChainInvoker>());
            new RouteMapper(requestGraph.HttpConfiguration, 
                    requestGraph.Container,
                    new DefaultInlineConstraintResolver())
                .Map(requestGraph.GetActionDescriptor());
        }
    }
}
