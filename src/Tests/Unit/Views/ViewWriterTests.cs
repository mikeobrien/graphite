using System.Net.Http;
using System.Text;
using Graphite.Http;
using Graphite.Views;
using Graphite.Views.Engines;
using Graphite.Writers;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Views
{
    [TestFixture]
    public class ViewWriterTests
    {
        private ViewContent _viewContent;

        public class Model { public string Value { get; set; } }
        public class Handler { public Model Get() { return null; } }

        [SetUp]
        public void Setup()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(h => h.Get());
            _viewContent = new ViewContent(new View(null, () => "{{Value}}", 
                new [] { MimeTypes.TextHtml }, Encoding.UTF8, MimeTypes.TextHtml,
                requestGraph.GetActionDescriptor()), new MustacheEngine());
        }

        [Test]
        public void Should_write_view()
        {
            var response = new HttpResponseMessage();
            new ViewWriter(_viewContent, new HttpRequestMessage(), response)
                .Write(new ResponseWriterContext(new Model { Value = "fark" })).Wait();

            response.Content.ShouldNotBeNull();
            response.Content.ReadAsStringAsync().Result.ShouldEqual("fark");
            response.Content.Headers.ContentType.MediaType
                .ShouldEqual(MimeTypes.TextHtml);
        }
    }
}
