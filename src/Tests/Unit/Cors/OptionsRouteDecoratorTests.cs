using System.Collections.Generic;
using Graphite.Cors;
using Graphite.Http;
using Graphite.Routing;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Cors
{
    [TestFixture]
    public class OptionsRouteDecoratorTests
    {
        [TestCase("GET", true)]
        [TestCase("OPTIONS", false)]
        public void Should_only_apply_to_routes_without_an_options_header(string existingMethod, bool applies)
        {
            var context = new HttpRouteDecoratorContext(new HttpRouteConfiguration
            {
                MethodConstraints = new List<string> { existingMethod }
            });

            new OptionsRouteDecorator().AppliesTo(context).ShouldEqual(applies);
        }

        [Test]
        public void Should_add_header()
        {
            var context = new HttpRouteDecoratorContext(new HttpRouteConfiguration
            {
                MethodConstraints = new List<string>()
            });

            new OptionsRouteDecorator().Decorate(context);

            context.Route.MethodConstraints.ShouldOnlyContain(HttpMethod.Options.Method);
        }
    }
}
