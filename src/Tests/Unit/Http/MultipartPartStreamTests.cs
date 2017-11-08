using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Graphite.Exceptions;
using Graphite.Http;
using NUnit.Framework;
using Should;
using Tests.Common;
using Graphite.Extensions;

namespace Tests.Unit.Http
{
    [TestFixture]
    public class MultipartPartStreamTests
    {
        private const string Boundry = "some-boundary";
        private Stream _stream;
        private MultipartReader _reader;
        private MultipartPartStream _multipartStream;

        [SetUp]
        public void Setup()
        {
            _stream = new MemoryStream();
            var content = new StringContent("fark")
            {
                Headers =
                {
                    ContentType = new MediaTypeHeaderValue(MimeTypes.MultipartFormData)
                    {
                        Parameters =
                        {
                            new NameValueHeaderValue("boundary", Boundry)
                        }
                    }
                }
            };
            _reader = new MultipartReader(_stream, content.Headers);
            _multipartStream = new MultipartPartStream(_reader);
        }

        [Test]
        public void Should_read_stream()
        {
            _stream.WriteAllText(
                "some preamble\r\n" +
                "--some-boundary\r\n" +
                "\r\n" +
                "some text\r\n" +
                "--some-boundary\r\n" +
                "\r\n" +
                "more text\r\n" +
                "--some-boundary--\r\n" +
                "some epilogue");
            _stream.Position = 0;

            _reader.ReadToNextPart();

            var value = "";
            var buffer = new byte[50];

            while (true)
            {
                var read = _multipartStream.Read(buffer, 10, 5);

                if (read == 0) break;

                value += buffer.ToString(10, read);
            }

            _multipartStream.Read(buffer, 10, 5).ShouldEqual(0);
            value.ShouldEqual("some text");
        }

        [Test]
        public void Should_fail_to_read_stream_with_bad_format()
        {
            _stream.WriteAllText(
                "--some-boundary" +
                "--some-boundary--");
            _stream.Position = 0;
            
            var buffer = new byte[50];

            var exception = _multipartStream.Should()
                .Throw<BadRequestException>(x => x.Read(buffer, 0, 50));

            exception.Message.ShouldNotBeEmpty();
        }
    }
}
