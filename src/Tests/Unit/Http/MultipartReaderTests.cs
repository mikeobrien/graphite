using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Graphite.Http;
using NUnit.Framework;
using Should;
using Graphite.Extensions;

namespace Tests.Unit.Http
{
    [TestFixture]
    public class MultipartReaderTests
    {
        [Test]
        public void Should_read_all_part_types()
        {
            var results = ReadAll(
                "some preamble\r\n" +
                "--some-boundary\r\n" +
                "content-type: text/plain\r\n" +
                "\r\n" +
                "some text\r\n" +
                "--some-boundary--\r\n" +
                "some epilogue");

            results.Count.ShouldEqual(6);

            Should_match_section(results[0], "some preamble", MultipartSection.Preamble, true, false);
            Should_match_section(results[1], "content-type", MultipartSection.Headers, false, false);
            Should_match_section(results[2], ": text/plain", MultipartSection.Headers, true, false);
            Should_match_section(results[3], "some text", MultipartSection.Body, true, false);
            Should_match_section(results[4], "some epilogue", MultipartSection.Epilogue, false, false);
            Should_match_section(results[5], "", MultipartSection.Epilogue, true, true);
        }

        [Test]
        public void Should_read_large_request()
        {
            var random = new Random();
            var file1 = new byte[1.MB()];
            var file2 = new byte[2.MB()];

            random.NextBytes(file1);
            random.NextBytes(file2);

            var results = ReadAll(
                "--some-boundary\r\n" +
                "\r\n" +
                $"{file1.ToString(1.MB(), Encoding.ASCII)}\r\n" +
                "--some-boundary\r\n" +
                "\r\n" +
                $"{file2.ToString(2.MB(), Encoding.ASCII)}\r\n" +
                "--some-boundary--", 750.KB());

            results.Count.ShouldEqual(8);

            Should_match_section(results[0], "", MultipartSection.Preamble, true, false);
            Should_match_section(results[1], file1.ToString(767975, Encoding.ASCII), MultipartSection.Body, false, false);
            Should_match_section(results[2], file1.ToString(767975, 280601, Encoding.ASCII), MultipartSection.Body, true, false);
            Should_match_section(results[3], file2.ToString(487372, Encoding.ASCII), MultipartSection.Body, false, false);
            Should_match_section(results[4], file2.ToString(487372, 767994, Encoding.ASCII), MultipartSection.Body, false, false);
            Should_match_section(results[5], file2.ToString(487372 + 767994, 767994, Encoding.ASCII), MultipartSection.Body, false, false);
            Should_match_section(results[6], file2.ToString(487372 + 767994 + 767994, 73792, Encoding.ASCII), MultipartSection.Body, true, false);
            Should_match_section(results[7], "", MultipartSection.Epilogue, true, true);
        }

        [Test]
        public void Should_read_part_with_no_headers([Values("", "\r\n")] string prefix)
        {
            var results = ReadAll(
                $"{prefix}--some-boundary\r\n" +
                "\r\n" +
                "some text\r\n" +
                "--some-boundary--");

            results.Count.ShouldEqual(3);

            Should_match_section(results[0], "", MultipartSection.Preamble, true, false);
            Should_match_section(results[1], "some text", MultipartSection.Body, true, false);
            Should_match_section(results[2], "", MultipartSection.Epilogue, true, true);
        }
        
        [Test]
        public void Should_read_part_with_no_headers_or_body()
        {
            var results = ReadAll(
                "--some-boundary\r\n" +
                "\r\n" +
                "\r\n" +
                "--some-boundary--");
            
            results.Count.ShouldEqual(3);

            Should_match_section(results[0], "", MultipartSection.Preamble, true, false);
            Should_match_section(results[1], "", MultipartSection.Body, true, false);
            Should_match_section(results[2], "", MultipartSection.Epilogue, true, true);
        }

        [Test]
        public void Should_read_multiple_parts_with_headers()
        {
            var results = ReadAll(
                "--some-boundary\r\n" +
                "content-type: text/plain\r\n" +
                "\r\n" +
                "some text 1\r\n" +
                "--some-boundary\r\n" +
                "content-type: text/plain\r\n" +
                "\r\n" +
                "some text 2\r\n" +
                "--some-boundary--");
            
            results.Count.ShouldEqual(8);

            Should_match_section(results[0], "", MultipartSection.Preamble, true, false);

            Should_match_section(results[1], "content-type: ", MultipartSection.Headers, false, false);
            Should_match_section(results[2], "text/plain", MultipartSection.Headers, true, false);
            Should_match_section(results[3], "some text 1", MultipartSection.Body, true, false);

            Should_match_section(results[4], "content-type: ", MultipartSection.Headers, false, false);
            Should_match_section(results[5], "text/plain", MultipartSection.Headers, true, false);
            Should_match_section(results[6], "some text 2", MultipartSection.Body, true, false);

            Should_match_section(results[7], "", MultipartSection.Epilogue, true, true);
        }

        [Test]
        public void Should_read_boundary_with_trailing_linear_whitespace()
        {
            var results = ReadAll(
                "--some-boundary \t\r \t\n\r\n" +
                "\r\n" +
                "some text\r\n" +
                "--some-boundary--");

            results.Count.ShouldEqual(3);

            Should_match_section(results[0], "", MultipartSection.Preamble, true, false);
            Should_match_section(results[1], "some text", MultipartSection.Body, true, false);
            Should_match_section(results[2], "", MultipartSection.Epilogue, true, true);
        }

        [Test]
        public void Should_fail_to_read_boundary_with_non_linear_whitespace()
        {
            var results = ReadAll(
                "--some-boundaryfark\r\n");

            results.Count.ShouldEqual(1);
            
            Should_match_section(results[0], "", null, true, false, true, 
                MultipartReader.ErrorInvalidCharactersFollowingBoundary);
        }

        [Test]
        public void Should_fail_to_read_headers_with_no_following_crlf()
        {
            var results = ReadAll(
                "--some-boundary\r\n" +
                "\r\n");

            results.Count.ShouldEqual(2);
            
            Should_match_section(results[0], "", MultipartSection.Preamble, true, false);
            Should_match_section(results[1], "", null, true, true, true, 
                MultipartReader.ErrorUnexpectedEndOfStream);
        }

        [Test]
        public void Should_fail_to_read_epilogue_containing_boundary()
        {
            var results = ReadAll(
                "--some-boundary\r\n" +
                "\r\n" +
                "\r\n" +
                "--some-boundary--\r\n" + 
                "--some-boundary");

            results.Count.ShouldEqual(3);
            
            Should_match_section(results[0], "", MultipartSection.Preamble, true, false);
            Should_match_section(results[1], "", MultipartSection.Body, true, false);
            Should_match_section(results[2], "", MultipartSection.Epilogue, false, false, true, 
                MultipartReader.ErrorBoundaryFoundAfterClosingBoundary);
        }

        [Test]
        public void Should_fail_to_read_first_boundary_not_preceeded_by_crlf()
        {
            var results = ReadAll(
                "fark--some-boundary");

            results.Count.ShouldEqual(2);
            
            Should_match_section(results[0], "far", MultipartSection.Preamble, false, false);
            Should_match_section(results[1], "", null, false, true, true, 
                MultipartReader.ErrorBoundaryNotPreceededByCRLF);
        }

        [Test]
        public void Should_fail_to_read_request_with_no_opening_boundary([Values("", "fark")] string data)
        {
            var results = ReadAll(data);

            results.Count.ShouldEqual(1);
            
            Should_match_section(results[0], data, MultipartSection.Preamble, true, true, true, 
                MultipartReader.ErrorUnexpectedEndOfStream);
        }

        [Test]
        public void Should_fail_to_read_boundary_not_preceeded_by_crlf()
        {
            var results = ReadAll(
                "--some-boundary\r\n" +
                "\r\n" +
                "--some-boundary--");

            results.Count.ShouldEqual(2);
            
            Should_match_section(results[0], "", MultipartSection.Preamble, true, false);
            Should_match_section(results[1], "", null, false, false, true, 
                MultipartReader.ErrorBoundaryNotPreceededByCRLF);
        }

        [Test]
        public void Should_fail_to_read_read_headers_missing_trailing_crlf()
        {
            var results = ReadAll(
                "--some-boundary\r\n" +
                "--some-boundary\r\n");

            results.Count.ShouldEqual(2);

            Should_match_section(results[0], "", MultipartSection.Preamble, true, false);
            Should_match_section(results[1], "", null, false, false, true, 
                MultipartReader.ErrorHeadersNotFollowedByEmptyLine);
        }

        [Test]
        public void Should_fail_to_read_read_end_of_request_following_headers()
        {
            var results = ReadAll(
                "--some-boundary\r\n");

            results.Count.ShouldEqual(2);

            Should_match_section(results[0], "", MultipartSection.Preamble, true, false);
            Should_match_section(results[1], "", null, true, true, true, 
                MultipartReader.ErrorUnexpectedEndOfStream);
        }

        [Test]
        public void Should_fail_to_read_boundary_without_trailing_crlf()
        {
            var results = ReadAll(
                "--some-boundary" +
                "--some-boundary--");

            results.Count.ShouldEqual(1);
            
            Should_match_section(results[0], "", null, true, false, true, 
                MultipartReader.ErrorUnexpectedEndOfStream);
        }

        [Test]
        public void Should_fail_to_read_epilogue_only_boundary([Values("", "\r\n")] string prefix)
        {
            var results = ReadAll($"{prefix}--some-boundary--");

            results.Count.ShouldEqual(1);
            
            Should_match_section(results[0], "", null, true, false, true, 
                MultipartReader.ErrorUnexpectedEndOfStream);
        }

        [Test]
        public void Should_fail_to_read_request_without_closing_boundary([Values("", "\r\n")] string prefix)
        {
            var results = ReadAll($"{prefix}--some-boundary");

            results.Count.ShouldEqual(1);
            
            Should_match_section(results[0], "", null, true, false, true, 
                MultipartReader.ErrorUnexpectedEndOfStream);
        }

        [Test]
        public void Should_fail_to_read_part_with_no_closing_boundary([Values("", "\r\n")] string postfix)
        {
            var results = ReadAll(
                "--some-boundary\r\n" +
                "\r\n" +
                $"some text{postfix}");

            results.Count.ShouldEqual(2);
            
            Should_match_section(results[0], "", MultipartSection.Preamble, true, false);
            Should_match_section(results[1], $"some text{postfix}", null, true, true, true, 
                MultipartReader.ErrorUnexpectedEndOfStream);
        }

        private void Should_match_section(ReadResult result,
            string value, MultipartSection? section,
            bool endOfPart, bool endOfStream, bool error = false,
            string errorMessage = null)
        {
            result.Data.ShouldEqual(value);
            result.Result.Section.ShouldEqual(section);
            result.Result.Read.ShouldEqual(value?.Length ?? 0);
            result.Result.EndOfPart.ShouldEqual(endOfPart);
            result.Result.EndOfStream.ShouldEqual(endOfStream);
            result.Result.Error.ShouldEqual(error);
            result.Result.ErrorMessage.ShouldEqual(errorMessage);
        }

        public class ReadResult
        {
            public MultipartReader.ReadResult Result { get; set; }
            public string Data { get; set; }
        }

        private List<ReadResult> ReadAll(string source, int bufferSize = 20)
        {
            var content = new StringContent(source);
            content.Headers.ContentType.Parameters.Add(
                new NameValueHeaderValue("boundary", "some-boundary"));
            var reader =  new MultipartReader(content.ReadAsStreamAsync()
                .Result, content.Headers, bufferSize + 10);
            var results = new List<ReadResult>();
            var buffer = new byte[bufferSize];
            var outputCount = 0;

            while (true)
            {
                var result = reader.Read(buffer, 0, buffer.Length);

                var data = "";
                if (result.Read > 0)
                    data = buffer.ToString(result.Read);
                if (outputCount < 100)
                {
                    outputCount++;
                    Console.WriteLine($"{result.Section.ToString().PadRight(8)}: " +
                        $"{(data?.Left(15) ?? "").PadRight(20)}  " +
                        $" {data?.Length.ToString().PadLeft(10)}   " +
                        $"EOP: {result.EndOfPart.ToString().PadRight(5)} " +
                        $"EOS: {result.EndOfStream.ToString().PadRight(5)} " +
                        $"Error: {result.ErrorMessage}");
                }
                results.Add(new ReadResult
                {
                    Result = result,
                    Data = data
                });
                if (result.EndOfStream || result.Error) return results;
            }
        }
    }
}
