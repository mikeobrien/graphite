using System;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Graphite.Http;
using NUnit.Framework;
using Should;
using TestHarness.Reader;
using Tests.Common;

namespace Tests.Acceptance
{
    [TestFixture]
    public class ReaderTests
    {
        private const string BaseUrl = "Reader/";

        [Test]
        public void Should_post_xml([Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).PostXml<ReaderTestHandler.XmlModel, 
                    ReaderTestHandler.XmlModel>($"{BaseUrl}Xml",
                new ReaderTestHandler.XmlModel { Value = "fark" });

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.ContentType.ShouldEqual(MimeTypes.ApplicationXml);
            result.Data.Value.ShouldEqual("fark");
        }

        [TestCase("Stream1", null, null, null, Host.IISExpress)]
        [TestCase("Stream2", "application/video", "weddingsinger.mp4", 4, Host.IISExpress)]

        [TestCase("Stream1", null, null, null, Host.Owin)]
        [TestCase("Stream2", "application/video", "weddingsinger.mp4", 4, Host.Owin)]
        public void Should_post_stream(string url, string mimetype, 
            string filename, int? length, Host host)
        {
            var result = Http.ForHost(host).PostStream<ReaderTestHandler.OutputInfoModel>(BaseUrl + url,
                new MemoryStream(Encoding.UTF8.GetBytes("fark")),
                "application/video", "weddingsinger.mp4");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Data.ShouldEqual("fark");
            result.Data.Filename.ShouldEqual(filename);
            result.Data.MimeType.ShouldEqual(mimetype);
            result.Data.Length.ShouldEqual(length);
        }

        [TestCase("String1", null, null, null, Host.IISExpress)]
        [TestCase("String2", "application/video", "weddingsinger.mp4", 4, Host.IISExpress)]

        [TestCase("String1", null, null, null, Host.Owin)]
        [TestCase("String2", "application/video", "weddingsinger.mp4", 4, Host.Owin)]
        public void Should_post_string(string url, string mimetype, 
            string filename, int? length, Host host)
        {
            var result = Http.ForHost(host).PostStream<ReaderTestHandler.OutputInfoModel>(BaseUrl + url,
                new MemoryStream(Encoding.UTF8.GetBytes("fark")),
                "application/video", "weddingsinger.mp4");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Data.ShouldEqual("fark");
            result.Data.Filename.ShouldEqual(filename);
            result.Data.MimeType.ShouldEqual(mimetype);
            result.Data.Length.ShouldEqual(length);
        }

        [TestCase("Bytes1", null, null, null, Host.IISExpress)]
        [TestCase("Bytes2", "application/video", "weddingsinger.mp4", 4, Host.IISExpress)]

        [TestCase("Bytes1", null, null, null, Host.Owin)]
        [TestCase("Bytes2", "application/video", "weddingsinger.mp4", 4, Host.Owin)]
        public void Should_post_bytes(string url, string mimetype, 
            string filename, int? length, Host host)
        {
            var result = Http.ForHost(host).PostStream<ReaderTestHandler.OutputInfoModel>(BaseUrl + url,
                new MemoryStream(Encoding.UTF8.GetBytes("fark")),
                "application/video", "weddingsinger.mp4");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.Data.ShouldEqual("fark");
            result.Data.Filename.ShouldEqual(filename);
            result.Data.MimeType.ShouldEqual(mimetype);
            result.Data.Length.ShouldEqual(length);
        }

        [Test]
        public void Should_return_400_if_no_reader_applies_to_request(
            [Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).Post($"{BaseUrl}NoReader",
                contentHeaders: x =>
                {
                    x.Clear();
                    x.ContentType = new MediaTypeWithQualityHeaderValue("fark/farker");
                });

            result.Status.ShouldEqual(HttpStatusCode.BadRequest);
            result.StatusText.ShouldEqual("Request format not supported.");
        }
    }
}
