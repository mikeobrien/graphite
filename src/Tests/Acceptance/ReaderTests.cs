using System.IO;
using System.Net;
using System.Text;
using Graphite.Http;
using NUnit.Framework;
using Should;
using TestHarness.Reader;
using WebClient = Tests.Common.WebClient;

namespace Tests.Acceptance
{
    [TestFixture]
    public class ReaderTests
    {
        private const string BaseUrl = "Reader/";

        [Test]
        public void Should_post_xml()
        {
            var result = WebClient.PostXml<ReaderTestHandler.XmlModel, 
                    ReaderTestHandler.XmlModel>($"{BaseUrl}Xml",
                new ReaderTestHandler.XmlModel { Value = "fark" });

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.ContentType.ShouldEqual(MimeTypes.ApplicationXml);
            result.Data.Value.ShouldEqual("fark");
        }

        [TestCase("Stream1", null, null, null)]
        [TestCase("Stream2", "application/video", "weddingsinger.mp4", 4)]
        public void Should_post_stream(string url, string mimetype, string filename, int? length)
        {
            var result = WebClient.PostStream<ReaderTestHandler.OutputInfoModel>(BaseUrl + url,
                new MemoryStream(Encoding.UTF8.GetBytes("fark")),
                "application/video", "weddingsinger.mp4");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Data.ShouldEqual("fark");
            result.Data.Filename.ShouldEqual(filename);
            result.Data.MimeType.ShouldEqual(mimetype);
            result.Data.Length.ShouldEqual(length);
        }

        [TestCase("String1", null, null, null)]
        [TestCase("String2", "application/video", "weddingsinger.mp4", 4)]
        public void Should_post_string(string url, string mimetype, string filename, int? length)
        {
            var result = WebClient.PostStream<ReaderTestHandler.OutputInfoModel>(BaseUrl + url,
                new MemoryStream(Encoding.UTF8.GetBytes("fark")),
                "application/video", "weddingsinger.mp4");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Data.ShouldEqual("fark");
            result.Data.Filename.ShouldEqual(filename);
            result.Data.MimeType.ShouldEqual(mimetype);
            result.Data.Length.ShouldEqual(length);
        }

        [TestCase("Bytes1", null, null, null)]
        [TestCase("Bytes2", "application/video", "weddingsinger.mp4", 4)]
        public void Should_post_bytes(string url, string mimetype, string filename, int? length)
        {
            var result = WebClient.PostStream<ReaderTestHandler.OutputInfoModel>(BaseUrl + url,
                new MemoryStream(Encoding.UTF8.GetBytes("fark")),
                "application/video", "weddingsinger.mp4");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Data.ShouldEqual("fark");
            result.Data.Filename.ShouldEqual(filename);
            result.Data.MimeType.ShouldEqual(mimetype);
            result.Data.Length.ShouldEqual(length);
        }
    }
}
