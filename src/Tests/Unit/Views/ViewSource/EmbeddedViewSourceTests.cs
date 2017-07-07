using System;
using System.Linq;
using System.Text;
using Graphite.Extensions;
using Graphite.Http;
using Graphite.Views;
using Graphite.Views.ViewSource;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Views.ViewSource
{
    [TestFixture]
    public class EmbeddedViewSourceTests
    {
        private RequestGraph _requestGraph;
        private ViewConfiguration _configuration;
        private EmbeddedViewSource _viewSource;

        public class Model { }
        public class Handler { public Model Post() { return null; } }

        [SetUp]
        public void Setup()
        {
            _configuration = new ViewConfiguration();
            _viewSource = new EmbeddedViewSource(_configuration);
            _requestGraph = RequestGraph.CreateFor<Handler>(h => h.Post());
        }

        [Test]
        public void Should_get_matching_resources()
        {
            var actionDescriptor = _requestGraph.GetActionDescriptor();
            var results = _viewSource.GetViews(
                new ViewSourceContext(actionDescriptor, 
                new [] { "fark", "farker", "farkest" }));

            results.Length.ShouldEqual(2);

            var view = results.First();

            view.Type.ShouldEqual("fark");
            view.Action.ShouldEqual(actionDescriptor);
            view.AcceptTypes.ShouldOnlyContain(MimeTypes.TextHtml);
            view.ContentType.ShouldEqual(MimeTypes.TextHtml);
            view.Encoding.ShouldEqual(Encoding.UTF8);
            view.Hash.ShouldEqual("source1".Hash());
            view.ModelType.Type.ShouldEqual(typeof(Model));
            view.Source.ShouldEqual("source1");

            view = results.Second();

            view.Type.ShouldEqual("farker");
            view.Action.ShouldEqual(actionDescriptor);
            view.AcceptTypes.ShouldOnlyContain(MimeTypes.TextHtml);
            view.ContentType.ShouldEqual(MimeTypes.TextHtml);
            view.Encoding.ShouldEqual(Encoding.UTF8);
            view.Hash.ShouldEqual("source2".Hash());
            view.ModelType.Type.ShouldEqual(typeof(Model));
            view.Source.ShouldEqual("source2");
        }
    }
}
