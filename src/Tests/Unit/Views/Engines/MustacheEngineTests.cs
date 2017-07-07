using Graphite.Views;
using Graphite.Views.Engines;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Views.Engines
{
    [TestFixture]
    public class MustacheEngineTests
    {
        private View _view;
        private MustacheEngine _engine;

        public class Model { public string Value { get; set; } }
        public class Handler { public Model Get() { return null; }}

        [SetUp]
        public void Setup()
        {
            _engine = new MustacheEngine();
            var requestGraph = RequestGraph.CreateFor<Handler>(h => h.Get());
            _view = new View(null, () => "{{Value}}", null, null, null,
                requestGraph.GetActionDescriptor());
        }

        [Test]
        public void Should_not_fail_to_precompile()
        {
            _engine.Should().NotThrow(x => x.PreCompile(_view));
        }

        [Test]
        public void Should_render_template()
        {
            _engine.Render(_view, new Model { Value = "fark" })
                .ShouldEqual("fark");
        }
    }
}
