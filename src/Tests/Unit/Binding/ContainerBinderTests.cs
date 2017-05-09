using System.Linq;
using System.Threading.Tasks;
using Graphite.Binding;
using NUnit.Framework;
using Should;
using Tests.Common;
using Graphite.DependencyInjection;
using Graphite.Extensions;

namespace Tests.Unit.Binding
{
    [TestFixture]
    public class ContainerBinderTests
    {
        public class Dependency { }

        public class Model
        {
            public Dependency Value { get; set; }
        }

        public class AttributeModel
        {
            [FromContainer]
            public Dependency Value { get; set; }
        }

        public class Handler
        {
            public void ComplexParams(Dependency dependency) { }
            public void ComplexAttributeParams([FromContainer] Dependency dependency) { }
            public void ComplexModelAttributeParams(AttributeModel model) { }
            public void ComplexModelParams(Model model) { }
            public void SimpleParams(string param) { }
        }

        [Test]
        public void Should_apply_if_there_are_complex_parmeters()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.ComplexParams(null))
                .AddAllActionParameters()
                .Configure(x => x.BindContainer());

            CreateBinder(requestGraph)
                .AppliesTo(requestGraph.GetRequestBinderContext())
                .ShouldBeTrue();
        }

        [Test]
        public void Should_apply_if_there_are_complex_model_parmeters()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.ComplexModelParams(null))
                .AddAllActionParameters(true)
                .Configure(x => x.BindContainer());

            requestGraph.RemoveParameter("model").ShouldBeTrue();

            CreateBinder(requestGraph)
                .AppliesTo(requestGraph.GetRequestBinderContext())
                .ShouldBeTrue();
        }

        [Test]
        public void Should_apply_if_configured_explicitly_and_parameter_marked()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.ComplexAttributeParams(null))
                .AddAllActionParameters()
                .Configure(x => x.BindContainerByAttribute());

            CreateBinder(requestGraph)
                .AppliesTo(requestGraph.GetRequestBinderContext())
                .ShouldBeTrue();
        }

        [Test]
        public void Should_apply_if_configured_explicitly_and_parameter_property_marked()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.ComplexModelAttributeParams(null))
                .AddAllActionParameters(true)
                .Configure(x => x.BindContainerByAttribute());

            requestGraph.RemoveParameter("model").ShouldBeTrue();

            CreateBinder(requestGraph)
                .AppliesTo(requestGraph.GetRequestBinderContext())
                .ShouldBeTrue();
        }

        [Test]
        public void Should_not_apply_if_there_are_not_complex_parmeters()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.SimpleParams(null))
                .AddAllActionParameters()
                .Configure(x => x.BindContainer());

            CreateBinder(requestGraph)
                .AppliesTo(requestGraph.GetRequestBinderContext())
                .ShouldBeFalse();
        }

        [Test]
        public void Should_not_apply_if_not_configured()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.ComplexParams(null))
                .AddAllActionParameters();

            CreateBinder(requestGraph)
                .AppliesTo(requestGraph.GetRequestBinderContext())
                .ShouldBeFalse();
        }

        [Test]
        public void Should_not_apply_if_configured_explicitly_and_parameter_not_marked()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.ComplexParams(null))
                .AddAllActionParameters()
                .Configure(x => x.BindContainerByAttribute());

            CreateBinder(requestGraph)
                .AppliesTo(requestGraph.GetRequestBinderContext())
                .ShouldBeFalse();
        }

        [Test]
        public async Task Should_bind_parameter_to_container_values()
        {
            var dependency = new Dependency();
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.ComplexParams(null))
                    .AddAllActionParameters()
                    .ConfigureContainer(r => r.Register(dependency))
                    .Configure(x => x.BindContainer());

            await CreateBinder(requestGraph).Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments[0].ShouldEqual(dependency);
        }

        [Test]
        public async Task Should_bind_parameter_property_to_container_values()
        {
            var dependency = new Dependency();
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.ComplexModelParams(null))
                .AddAllActionParameters(true)
                .ConfigureContainer(r => r.Register(dependency))
                .Configure(x => x.BindContainer());

            await CreateBinder(requestGraph).Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments[0].As<Model>().Value.ShouldEqual(dependency);
        }

        public ContainerBinder CreateBinder(RequestGraph requestGraph)
        {
            return new ContainerBinder(
                requestGraph.Configuration,
                requestGraph.GetRouteDescriptor(),
                requestGraph.Container);
        }
    }
}
