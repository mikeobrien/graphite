using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Graphite;
using Graphite.Exceptions;
using Graphite.Extensions;
using NUnit.Framework;
using Should;
using Tests.Common;
using MultipartContent = Graphite.Binding.MultipartContent;

namespace Tests.Unit.Binding
{
    [TestFixture]
    public class MultipartContentTests
    {
        private const string SinglePartContent =
            "--some-boundary\r\n" +
            "content-type: text/plain\r\n" +
            "\r\n" +
            "some text\r\n" +
            "--some-boundary--";

        private const string TwoPartContent =
            "--some-boundary\r\n" +
            "content-type: text/plain\r\n" +
            "content-length: 5\r\n" +
            "content-language: en, fr\r\n" +
            "content-encoding: gzip, base64\r\n" +
            "Content-Disposition: form-data; name=\"somename1\"; filename=\"somefile1.jpg\"\r\n" +
            "some-header: value1\r\n" +
            "\r\n" +
            "first part\r\n" +
            "--some-boundary\r\n" +
            "content-type: text/html\r\n" +
            "content-length: 6\r\n" +
            "content-language: gr, sw\r\n" +
            "content-encoding: farkzip, base2\r\n" +
            "Content-Disposition: form-data; name=\"somename2\"; filename=\"somefile2.jpg\"\r\n" +
            "some-header: value2\r\n" +
            "\r\n" +
            "second part\r\n" +
            "--some-boundary--";

        [Test]
        public void Should_peek_part()
        {
            var content = CreateContent(SinglePartContent);

            var part = content.Peek();

            part.ShouldEqual(content.Peek());

            part.ReadComplete.ShouldBeFalse();

            part.Headers.ContentType.MediaType.ShouldEqual("text/plain");

            part.ReadAsStringAsync().Result.ShouldEqual("some text");

            part.ReadComplete.ShouldBeTrue();
        }
        
        [Test]
        public void Should_return_null_when_peeking_at_end_of_string()
        {
            var content = CreateContent(SinglePartContent);

            content.Pop().ShouldNotBeNull();

            content.Peek().ShouldBeNull();
        }
        
        [Test]
        public void Should_return_null_when_popping_at_end_of_string()
        {
            var content = CreateContent(SinglePartContent);

            content.Pop().ShouldNotBeNull();

            content.Pop().ShouldBeNull();
        }
        
        [Test]
        public void Should_pop_part()
        {
            var content = CreateContent(SinglePartContent);

            var part = content.Pop();

            part.ReadComplete.ShouldBeFalse();

            part.Headers.ContentType.MediaType.ShouldEqual("text/plain");

            part.ReadAsStringAsync().Result.ShouldEqual("some text");

            part.ReadComplete.ShouldBeTrue();

            part.ShouldNotEqual(content.Pop());
        }
        
        [Test]
        public void Should_pop_peeked_part()
        {
            var content = CreateContent(SinglePartContent);

            var part = content.Peek();

            part.ShouldEqual(content.Pop());

            part.ShouldNotEqual(content.Pop());
        }
        
        [Test]
        public void Should_get_stream_enumeration()
        {
            CreateContent(TwoPartContent).GetStreams()
                .Select(x => x.ReadToEnd()).ToList()
                .ShouldOnlyContain("first part", "second part");
        }
        
        [Test]
        public void Should_should_get_input_stream_enumeration()
        {
            var results = CreateContent(TwoPartContent)
                .Select(x => new
                {
                    x.ContentType,
                    x.Encoding,
                    x.Filename,
                    x.Headers,
                    x.Length,
                    x.Name,
                    Data = x.Data.ReadToEnd()
                }).ToList();

            results.Count.ShouldEqual(2);

            var inputStream = results[0];

            inputStream.ContentType.ShouldEqual("text/plain");
            inputStream.Encoding.ShouldOnlyContain("gzip", "base64");
            inputStream.Filename.ShouldEqual("somefile1.jpg");
            inputStream.Length.ShouldEqual(5);
            inputStream.Name.ShouldEqual("somename1");

            inputStream.Headers.Count().ShouldEqual(6);

            inputStream.Headers.GetValues("some-header").ShouldOnlyContain("value1");

            inputStream = results[1];

            inputStream.ContentType.ShouldEqual("text/html");
            inputStream.Encoding.ShouldOnlyContain("farkzip", "base2");
            inputStream.Filename.ShouldEqual("somefile2.jpg");
            inputStream.Length.ShouldEqual(6);
            inputStream.Name.ShouldEqual("somename2");

            inputStream.Headers.Count().ShouldEqual(6);

            inputStream.Headers.GetValues("some-header").ShouldOnlyContain("value2");
        }
        
        [Test]
        public void Should_get_enumeration_multiple_times()
        {
            var content = CreateContent(TwoPartContent);
                
            content.GetStreams()
                .Select(x => x.ReadToEnd()).First()
                .ShouldEqual("first part");
                
            content.GetStreams()
                .Select(x => x.ReadToEnd()).First()
                .ShouldEqual("second part");
        }
        
        [Test]
        public void Should_should_get_stream_and_input_stream_enumeration()
        {
            var content = CreateContent(TwoPartContent);
                
            content.GetStreams()
                .Select(x => x.ReadToEnd()).First()
                .ShouldEqual("first part");
                
            content
                .Select(x => x.Data.ReadToEnd()).First()
                .ShouldEqual("second part");
        }
        
        [Test]
        public void Should_exclude_preamble_and_epilogue()
        {
            CreateContent(
                    "some preamble\r\n" +
                    "--some-boundary\r\n" +
                    "\r\n" +
                    "first part\r\n" +
                    "--some-boundary\r\n" +
                    "\r\n" +
                    "second part\r\n" +
                    "--some-boundary--\r\n" +
                    "some epilogue")
                .GetStreams()
                .Select(x => x.ReadToEnd()).ToList()
                .ShouldOnlyContain("first part", "second part");
        }
        
        [Test]
        public void Should_read_to_next_part_if_last_part_wasnt_fully_read()
        {
            var content = CreateContent(TwoPartContent);

            content.Pop();

            content.Pop().ReadAsStringAsync().Result
                .ShouldEqual("second part");
        }
        
        [Test]
        public void Should_throw_bad_request_exception_on_enueration_error()
        {
            var exception = CreateContent(
                "--some-boundary\r\n" +
                "--some-boundary--")
                .Should().Throw<BadRequestException>(x => x.First());

            exception.Message.ShouldNotBeEmpty();
        }
        
        [Test]
        public void Should_return_error_when_reading_to_next_part()
        {
            var content = CreateContent(
                "--some-boundary\r\n" +
                "\r\n" +
                "--some-boundary--");
                
            var result = content.Pop();

            result.Error.ShouldBeFalse();

            result = content.Pop();

            result.Error.ShouldBeTrue();
            result.ErrorMessage.ShouldNotBeEmpty();
        }
        
        [Test]
        public void Should_return_error_when_reading_headers()
        {
            var result = CreateContent(
                    "--some-boundary\r\n" +
                    "--some-boundary--").Pop();

            result.Error.ShouldBeTrue();
            result.ErrorMessage.ShouldNotBeEmpty();
        }

        private MultipartContent CreateContent(string content)
        {
            var request = new StringContent("");
            request.Headers.ContentType = 
                new MediaTypeHeaderValue("multipart/form-data")
                {
                    Parameters =
                    {
                        new NameValueHeaderValue("boundary", "some-boundary")
                    }
                };
            return new MultipartContent(content.ToStream(), 
                request.Headers, new Configuration());
        }
    }
}
