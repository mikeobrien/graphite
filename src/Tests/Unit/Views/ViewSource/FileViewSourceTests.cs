using System;
using System.IO;
using System.Linq;
using System.Text;
using Graphite.Extensions;
using Graphite.Hosting;
using Graphite.Http;
using Graphite.Views;
using Graphite.Views.ViewSource;
using NSubstitute;
using NUnit.Framework;
using Should;
using Tests.Common;
using Tests.Unit.Views.ViewSource;

public class FileViewSourceNoNamespace
{
    public FileViewSourceTests.Model Post() { return null; }
}

namespace Tests.Unit.Views.ViewSource
{
    [TestFixture]
    public class FileViewSourceTests
    {
        private string _tempPath;
        private ViewConfiguration _configuration;
        private FileViewSource _viewSource;

        public class Model { }
        public class Handler { public Model Post() { return null; } }

        [SetUp]
        public void Setup()
        {
            _tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            _configuration = new ViewConfiguration();
            var pathProvider = Substitute.For<IPathProvider>();
            pathProvider.MapPath(Arg.Any<string>()).ReturnsForAnyArgs(_tempPath);
            _viewSource = new FileViewSource(_configuration, pathProvider);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempPath))
                Directory.Delete(_tempPath, true);
        }

        [Test]
        public void Should_return_existing_Views()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(h => h.Post());
            var actionDescriptor = requestGraph.GetActionDescriptor();
            WriteTemplate(@"Unit\Views\ViewSource", 
                $"{nameof(FileViewSourceTests)}.{nameof(Handler)}", 
                "fark", "source1");
            WriteTemplate(@"Unit\Views\ViewSource", 
                $"{nameof(FileViewSourceTests)}.{nameof(Model)}", 
                "farker", "source2");
            var results = _viewSource.GetViews(
                new ViewSourceContext(requestGraph.GetActionDescriptor(),
                    new[] { "fark", "farker", "farkest" }));

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

        [Test]
        public void Should_not_fail_when_handler_has_no_namespace()
        {
            var requestGraph = RequestGraph.CreateFor<FileViewSourceNoNamespace>(h => h.Post());
            var actionDescriptor = requestGraph.GetActionDescriptor();
            WriteTemplate(null, nameof(FileViewSourceNoNamespace), "fark", "farker");
            var results = _viewSource.GetViews(
                new ViewSourceContext(actionDescriptor,
                    new[] { "fark", "farker", "farkest" }));

            results.Length.ShouldEqual(1);

            var view = results.First();

            view.Type.ShouldEqual("fark");
            view.Action.ShouldEqual(actionDescriptor);
            view.AcceptTypes.ShouldOnlyContain(MimeTypes.TextHtml);
            view.ContentType.ShouldEqual(MimeTypes.TextHtml);
            view.Encoding.ShouldEqual(Encoding.UTF8);
            view.Hash.ShouldEqual("farker".Hash());
            view.ModelType.Type.ShouldEqual(typeof(Model));
            view.Source.ShouldEqual("farker");
        }

        private void WriteTemplate(string relativePath, string name, string type, string content)
        {
            var folder = relativePath.IsNotNullOrEmpty() 
                ? Path.Combine(_tempPath, relativePath)
                : _tempPath;
            Directory.CreateDirectory(folder);
            var filename = $"{name}.{type}";
            File.WriteAllText(Path.Combine(folder, filename), content);
        }
    }
}
