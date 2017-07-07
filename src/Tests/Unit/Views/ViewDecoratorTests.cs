using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;
using Graphite;
using Graphite.Actions;
using Graphite.Http;
using Graphite.Views;
using Graphite.Views.Engines;
using Graphite.Views.ViewSource;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Views
{
    [TestFixture]
    public class ViewDecoratorTests
    {
        private Configuration _configuration;
        private ViewConfiguration _viewConfiguration;
        private List<IViewSource> _viewSources;
        private List<IViewEngine> _viewEngines;
        private ViewDecorator _decorator;

        [SetUp]
        public void Setup()
        {
            _configuration = new Configuration();
            _viewConfiguration = new ViewConfiguration();
            var mustacheEngine = new MustacheEngine();
            _viewEngines = new List<IViewEngine> { mustacheEngine };
            _viewConfiguration.ViewEngines.Configure(x => x.Append(mustacheEngine));
            var embeddedViewSource = new EmbeddedViewSource(_viewConfiguration);
            _viewSources = new List<IViewSource> { embeddedViewSource };
            _viewConfiguration.ViewSources.Configure(x => x.Append(embeddedViewSource));
            _decorator = new ViewDecorator(_viewSources, _viewConfiguration,
                _viewEngines, _configuration, new HttpConfiguration());
        }

        public class Model { public string Value { get; set; }}
        public class Handler
        {
            public Model HasResponse() { return null; }
            public void NoResponse() { }
        }

        [Test]
        public void Should_not_apply_to_endpoints_without_a_reponse()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.NoResponse());
            var context = new ActionDecoratorContext(requestGraph.GetActionDescriptor());
            _decorator.AppliesTo(context).ShouldBeFalse();
        }

        [Test]
        public void Should_apply_to_endpoints_with_a_reponse()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.HasResponse());
            var context = new ActionDecoratorContext(requestGraph.GetActionDescriptor());
            _decorator.AppliesTo(context).ShouldBeTrue();
        }

        [Test]
        public void Should_decorate_actions([Values(true, false)] bool clearOtherWriters)
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.HasResponse());
            var actionDescriptor = requestGraph.GetActionDescriptor();
            var context = new ActionDecoratorContext(actionDescriptor);
            
            _viewConfiguration.ClearOtherWriters = clearOtherWriters;

            _decorator.Decorate(context);

            var views = actionDescriptor.Registry.Where(x => x.Instance is ViewContent).ToList();

            views.Count.ShouldEqual(1);

            var viewContent = views.First().Instance as ViewContent;

            viewContent.AcceptTypes.ShouldOnlyContain(MimeTypes.TextHtml);
            viewContent.Encoding.ShouldEqual(Encoding.UTF8);
            viewContent.Render(new Model { Value = "fark" }).ShouldEqual("fark");

            actionDescriptor.Registry.Any(x => x.ConcreteType == typeof(ViewWriter)).ShouldBeTrue();
            actionDescriptor.ResponseWriters.Any(x => x.Type == typeof(ViewWriter)).ShouldBeTrue();

            if (clearOtherWriters) actionDescriptor.ResponseWriters.Count().ShouldEqual(1);
            else actionDescriptor.ResponseWriters.Count().ShouldBeGreaterThan(1);
        }

        [Test]
        public void Should_not_decorate_actions_without_a_response()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.NoResponse());
            var actionDescriptor = requestGraph.GetActionDescriptor();
            var context = new ActionDecoratorContext(actionDescriptor);

            _viewConfiguration.ClearOtherWriters = true;

            _decorator.Decorate(context);

            Should_not_decorate(actionDescriptor);
        }

        [Test]
        public void Should_not_include_engines_that_dont_apply()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.HasResponse());
            var actionDescriptor = requestGraph.GetActionDescriptor();
            var context = new ActionDecoratorContext(actionDescriptor);

            _viewConfiguration.ViewEngines.Configure(x => x
                .Clear().Append<MustacheEngine>(y => false));

            _decorator.Decorate(context);

            Should_not_decorate(actionDescriptor);
        }

        [Test]
        public void Should_not_include_view_sources_that_dont_apply()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(x => x.HasResponse());
            var actionDescriptor = requestGraph.GetActionDescriptor();
            var context = new ActionDecoratorContext(actionDescriptor);

            _viewConfiguration.ViewSources.Configure(x => x
                .Clear().Append<EmbeddedViewSource>(y => false));

            _decorator.Decorate(context);

            Should_not_decorate(actionDescriptor);
        }

        private void Should_not_decorate(ActionDescriptor actionDescriptor)
        {
            actionDescriptor.Registry.Where(x => x.Instance is ViewContent).ShouldBeEmpty();
            actionDescriptor.Registry.Where(x => x.ConcreteType == typeof(ViewWriter)).ShouldBeEmpty();
            actionDescriptor.ResponseWriters.Where(x => x.Type == typeof(ViewWriter)).ShouldBeEmpty();
            actionDescriptor.ResponseWriters.Count().ShouldBeGreaterThan(1);
        }
    }
}
