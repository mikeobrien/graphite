using System.Linq;
using System.Web.Http.Routing;
using System.Web.Http.Routing.Constraints;
using Graphite.Routing;
using NUnit.Framework;
using Should;

namespace Tests.Unit.Routing
{
    [TestFixture]
    public class RouteExtensionTests
    {
        [TestCase("fark/{param:range(4, 8)}/farker", "fark/{param}/farker")]
        [TestCase("fark/{param:int:regex({\\:\\/})}/farker", "fark/{param}/farker")]
        public void Should_remove_constraints_from_route_template(string template, string expected)
        {
            template.RemoveConstraints().ShouldEqual(expected);
        }

        [TestCase("fark/{param:range(4, 8)}/farker", "fark/{:range(4, 8)}/farker")]
        [TestCase("fark/{param:regex({\\:\\/}):int}/farker", "fark/{:int:regex({\\:\\/})}/farker")]
        public void Should_remove_parameter_names_from_route_template(string template, string expected)
        {
            template.RemoveParameterNames().ShouldEqual(expected);
        }

        [Test]
        public void Should_get_parameter_constraints()
        {
            var template = "fark/{param1:range(4, 8)}/{param2}/farker/" +
                           "{param3:alpha}/{param4:int:range(5, 10)}";
            var constraints = new RouteDescriptor(null, template, null, null, null, null)
                .GetRouteConstraints(new DefaultInlineConstraintResolver());

            constraints.Count.ShouldEqual(3);

            var range = constraints["param1"] as RangeRouteConstraint;
            range.ShouldNotBeNull();
            range.Min.ShouldEqual(4);
            range.Max.ShouldEqual(8);

            var alpha = constraints["param3"] as AlphaRouteConstraint;
            alpha.ShouldNotBeNull();

            var composite = constraints["param4"] as CompoundRouteConstraint;
            composite.ShouldNotBeNull();

            var @int = composite.Constraints.First() as IntRouteConstraint;
            @int.ShouldNotBeNull();

            range = composite.Constraints.Skip(1).First() as RangeRouteConstraint;
            range.ShouldNotBeNull();
            range.Min.ShouldEqual(5);
            range.Max.ShouldEqual(10);
        }
    }
}
