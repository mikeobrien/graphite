using System;
using System.Linq;
using System.Linq.Expressions;
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
    public class ViewSourceBaseTests
    {
        private ViewConfiguration _configuration;

        [SetUp]
        public void Setup()
        {
            _configuration = new ViewConfiguration();
        }

        public class DefaultModel { }

        [ViewAccept("model/accept"), ViewContentType("model/content-type"),
         ViewEncoding("ascii"), View("model_view")]
        public class OverrideModel { }

        public class Handler
        {
            public DefaultModel Default() { return null; }

            [ViewAccept("action/accept"), ViewContentType("action/content-type"),
             ViewEncoding("utf-7"), View("action_view")]
            public DefaultModel OverrideAction() { return null; }

            public OverrideModel OverrideModel() { return null; }
        }

        [ViewAccept("handler/accept"), ViewContentType("handler/content-type"),
         ViewEncoding("unicode"), View("handler_view")]
        public class OverrideHandler { public DefaultModel Default() { return null; } }

        public static object[][] TestCases = TestCaseSource.Create
            <string, string, Encoding, string>(x => x
            .Add<Handler, DefaultModel>(h => h.Default(), MimeTypes.TextHtml, 
                MimeTypes.TextHtml, Encoding.UTF8,
                $"{nameof(ViewSourceBaseTests)}.{nameof(DefaultModel)}",
                $"{nameof(ViewSourceBaseTests)}.{nameof(Handler)}")

            .Add<OverrideHandler, DefaultModel>(h => h.Default(), "handler/accept",
                "handler/content-type", Encoding.Unicode, "handler_view")

            .Add<Handler, DefaultModel>(h => h.OverrideAction(), "action/accept",
                "action/content-type", Encoding.UTF7, "action_view")

            .Add<Handler, OverrideModel>(h => h.OverrideModel(), "model/accept",
                "model/content-type", Encoding.ASCII, "model_view"));

        [TestCaseSource(nameof(TestCases))]
        public void Should_return_default_accept_types(LambdaExpression expression, 
            Type modelType, string accept, string contentType, 
            Encoding encoding, string[] names)
        {
            var requestGraph = RequestGraph.CreateFor(expression);
            var actionDescriptor = requestGraph.GetActionDescriptor();
            var viewSource = new TestViewSource(_configuration, 
                new ViewDescriptor("fark", () => "farker"));
            var context = new ViewSourceContext(actionDescriptor, null);
            var results = viewSource.GetViews(context);

            viewSource.Context.ShouldEqual(context);
            viewSource.Encoding.ShouldEqual(encoding);
            viewSource.ViewNames.ShouldOnlyContain(names);

            results.Length.ShouldEqual(1);

            var view = results.First();

            view.Action.ShouldEqual(actionDescriptor);
            view.AcceptTypes.ShouldOnlyContain(accept);
            view.ContentType.ShouldEqual(contentType);
            view.Encoding.ShouldEqual(encoding);
            view.Hash.ShouldEqual("farker".Hash());
            view.ModelType.Type.ShouldEqual(modelType);
            view.Source.ShouldEqual("farker");
            view.Type.ShouldEqual("fark");
        }

        [Test]
        public void Should_return_default_view_names()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.Default());
            var context = new ViewSourceContext(requestGraph.GetActionDescriptor(), null);

            ViewSourceBase.DefaultViewNameConvention(context)
                .ShouldOnlyContain(
                    "ViewSourceBaseTests.DefaultModel", 
                    "ViewSourceBaseTests.Handler");
        }
        
        public class TestViewSource : ViewSourceBase
        {
            private readonly ViewDescriptor[] _viewDescriptors;

            public TestViewSource(ViewConfiguration viewConfiguration,
                params ViewDescriptor[] viewDescriptors) : 
                base(viewConfiguration)
            {
                _viewDescriptors = viewDescriptors;
            }

            public ViewSourceContext Context { get; private set; }
            public string[] ViewNames { get; private set; }
            public Encoding Encoding { get; private set; }

            protected override ViewDescriptor[] GetViewDescriptors(
                ViewSourceContext context, string[] viewNames, Encoding encoding)
            {
                Context = context;
                ViewNames = viewNames;
                Encoding = encoding;
                return _viewDescriptors;
            }
        }
    }
}
