using Graphite.Views;
using NUnit.Framework;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using Should;
using Tests.Common;

namespace Tests.Unit.Views.Engines
{
    [TestFixture]
    public class RazorEngineTests
    {
        private View _view;
        private Graphite.Views.Engines.RazorEngine _engine;

        public class Model { public string Value { get; set; } }
        public class Handler { public Model Get() { return null; } }

        [SetUp]
        public void Setup()
        {
            _engine = new Graphite.Views.Engines.RazorEngine(RazorEngineService.Create(
                new TemplateServiceConfiguration()));
            var requestGraph = RequestGraph.CreateFor<Handler>(h => h.Get());
            _view = new View(null, () => "@Model.Value", null, null, null,
                requestGraph.GetActionDescriptor());
        }
        
        [Test]
        public void Should_render_template()
        {
            _engine.PreCompile(_view);
            _engine.Render(_view, new Model { Value = "fark" })
                .ShouldEqual("fark");
        }
    }
}
