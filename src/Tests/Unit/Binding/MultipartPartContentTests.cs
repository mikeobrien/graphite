using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Graphite.Binding;
using Graphite.Http;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Binding
{
    [TestFixture]
    public class MultipartPartContentTests
    {
        [Test]
        public void Should_parse_headers()
        {
            var content = CreateContent(headers: 
                "content-type: text/plain\r\n" +
                "content-length: 5\r\n" +
                "content-language: en, fr\r\n" +
                "content-encoding: gzip, base64\r\n" +
                "Content-Disposition: form-data; name=\"somename\"; filename=\"somefile.jpg\"\r\n" +
                "some-header: value1, value2");

            var contentDisposition = content.Headers.ContentDisposition;
            contentDisposition.Name.ShouldEqual("somename");
            contentDisposition.DispositionType.ShouldEqual("form-data");
            contentDisposition.FileName.ShouldEqual("somefile.jpg");

            content.Name.ShouldEqual("somename");
            content.Type.ShouldEqual("form-data");
            content.Filename.ShouldEqual("somefile.jpg");
            
            content.Headers.ContentType.MediaType.ShouldEqual("text/plain");

            content.Headers.ContentEncoding.ShouldOnlyContain("gzip", "base64");

            content.Headers.ContentLength.ShouldEqual(5);

            content.Headers.ContentLanguage.ShouldOnlyContain("en", "fr");

            content.Headers.Count().ShouldEqual(6);

            content.Headers.GetValues("some-header")
                .ShouldOnlyContain("value1, value2");
        }

        [Test]
        public void Should_not_fail_to_parse_headers_if_null_or_empty(
            [Values(null, "")] string headers)
        {
            var content = CreateContent(headers: headers);

            content.Headers.ShouldBeEmpty();
        }

        [Test]
        public void Should_indicate_if_an_error_occured()
        {
            var content = new MultipartPartContent(null, null, true, "fark");

            content.Error.ShouldBeTrue();
            content.ErrorMessage.ShouldEqual("fark");
        }
        
        [Test]
        public void Should_read_as_async_stream()
        {
            var content = CreateContent("fark");
            
            content.ReadComplete.ShouldBeFalse();
            content.ReadAsStreamAsync().Result.ReadAllText().ShouldEqual("fark");
            content.ReadComplete.ShouldBeTrue();
        }
        
        [Test]
        public void Should_read_as_async_string()
        {
            var content = CreateContent("fark");
            
            content.ReadComplete.ShouldBeFalse();
            content.ReadAsStringAsync().Result.ShouldEqual("fark");
            content.ReadComplete.ShouldBeTrue();
        }

        private MultipartPartContent CreateContent(string content = "", string headers = "")
        {
            var request = new StringContent("");
            request.Headers.ContentType.Parameters.Add(
                new NameValueHeaderValue("boundary", "some-boundary"));
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(
                "--some-boundary\r\n" +
                "\r\n" +
                $"{content}\r\n" +
                "--some-boundary--"));
            var reader = new MultipartReader(stream, request.Headers);
            reader.ReadToNextPart();
            return new MultipartPartContent(reader, headers, false, null);
        }
    }
}
