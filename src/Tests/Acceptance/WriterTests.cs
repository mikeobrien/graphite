using System.Net;
using Graphite.Http;
using NUnit.Framework;
using Should;
using TestHarness.Writer;
using Tests.Common;
using WebClient = Tests.Common.WebClient;

namespace Tests.Acceptance
{
    [TestFixture]
    public class WriterTests
    {
        private const string BaseUrl = "Writer/";

        [Test]
        public void Should_get_xml()
        {
            var result = WebClient.GetXml<WriterTestHandler.XmlModel>($"{BaseUrl}Xml");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.ContentType.ShouldEqual(MimeTypes.ApplicationXml);
            result.Data.Value.ShouldEqual("fark");
        }

        [Test]
        public void Should_get_string([Values("String1", "String2")] string url)
        {
            var result = WebClient.GetStream(BaseUrl + url, data =>
            {
                data.ReadAllText().ShouldEqual("fark");
            });
            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Filename.ShouldEqual("weddingsinger.mp4");
            result.ContentType.ShouldEqual("application/video");
        }

        [Test]
        public void Should_get_stream([Values("Stream1", "Stream2")] string url)
        {
            var result = WebClient.GetStream(BaseUrl + url, data =>
            {
                data.ReadAllText().ShouldEqual("fark");
            });
            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Filename.ShouldEqual("weddingsinger.mp4");
            result.ContentType.ShouldEqual("application/video");
        }

        [Test]
        public void Should_get_bytes([Values("Bytes1", "Bytes2")] string url)
        {
            var result = WebClient.GetStream(BaseUrl + url, data =>
            {
                data.ReadAllText().ShouldEqual("fark");
            });
            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Filename.ShouldEqual("weddingsinger.mp4");
            result.ContentType.ShouldEqual("application/video");
        }

        [Test]
        public void Should_redirect([Values("Redirect",
            "RedirectModel?redirect=true")] string url)
        {
            var result = WebClient.GetString(BaseUrl + url);

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.ShouldContain("google");
        }

        [Test]
        public void Should_not_redirect_from_model_when_no_redirect_specified()
        {
            var result = WebClient.GetJson<WriterTestHandler.RedirectModel>(
                $"{BaseUrl}RedirectModel?redirect=false");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Value.ShouldEqual("value");
        }
    }
}
