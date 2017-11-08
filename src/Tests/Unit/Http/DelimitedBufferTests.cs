using System;
using System.IO;
using System.Linq;
using Graphite.Extensions;
using Graphite.Http;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Http
{
    [TestFixture]
    public class DelimitedBufferTests
    {
        //        Data    Buffer  Compare   Expected
        [TestCase("",     1,      "",       false)]
        [TestCase("fark", 1,      "f",      true)]
        [TestCase("fark", 1,      "fa",     false)]
        [TestCase("fark", 2,      "fa",     true)]
        [TestCase("fark", 4,      "fark",   true)]
        [TestCase("fark", 4,      "farker", false)]
        public void Should_start_with(string data, int bufferSize, string compare, bool expected)
        {
            var stream = new MemoryStream(data.ToBytes());
            var buffer = new DelimitedBuffer(stream, bufferSize);

            buffer.StartsWith(compare.ToBytes()).ShouldEqual(expected);
        }

        [Test]
        public void Should_not_start_with_comparison_that_exceeds_buffer_size()
        {
            var data = "fark".ToBytes();
            var stream = new MemoryStream(data);
            var buffer = new DelimitedBuffer(stream, 4);

            buffer.ReadTo(data);

            buffer.StartsWith(data).ShouldBeFalse();
        }
        
        //        Data     Buffer  Delimiter   Invalid EOP1    EOS1   Read1 Invalid1   EOP2   EOS2   Read2  Invalid2  Remaining
        [TestCase("",        1,    "",         "",     true,   true,  "",   false,     true,  true,  "",    false,    "")]
        [TestCase(",",       1,    ",",        "",     true,   false, "",   false,     true,  true,  "",    false,    "")]

        [TestCase("a",       1,    ",",        "",     false,  false,  "a", false,     true,  true,  "",    false,    "")]
        [TestCase("a",       2,    ",",        "",     false,  false,  "a", false,     true,  true,  "",    false,    "")]
        [TestCase("a",       3,    ",",        "",     false,  false,  "a", false,     true,  true,  "",    false,    "")]
        [TestCase("a",       4,    ",",        "",     false,  false,  "a", false,     true,  true,  "",    false,    "")]

        [TestCase("a,",      1,    ",",        "",     false,  false, "a",  false,     true,  false,  "",   false,    "")]
        [TestCase("a,",      2,    ",",        "",     true,   false, "a",  false,     true,  true,  "",    false,    "")]
        [TestCase("a,",      3,    ",",        "",     true,   false, "a",  false,     true,  true,  "",    false,    "")]
        [TestCase("a,",      4,    ",",        "",     true,   false, "a",  false,     true,  true,  "",    false,    "")]

        [TestCase("ab,",     1,    ",",        "a",    false,  false, "",   true,      false, false, "",    true,     "ab,")]
        [TestCase("ab,",     2,    ",",        "a",    false,  false, "",   true,      false, false, "",    true,     "ab,")]
        [TestCase("ab,",     3,    ",",        "a",    false,  false, "",   true,      false, false, "",    true,     "ab,")]
        [TestCase("ab,",     4,    ",",        "a",    false,  false, "",   true,      false, false, "",    true,     "ab,")]

        [TestCase("ab,",     1,    ",",        "b",    false,  false, "a",  false,     false, false, "",    true,     "b,")]
        [TestCase("ab,",     2,    ",",        "b",    false,  false, "",   true,      false, false, "",    true,     "ab,")]
        [TestCase("ab,",     3,    ",",        "b",    false,  false, "",   true,      false, false, "",    true,     "ab,")]
        [TestCase("ab,",     4,    ",",        "b",    false,  false, "",   true,      false, false, "",    true,     "ab,")]

        [TestCase("ab,",     1,    ",",        "",     false,  false, "a",  false,     false, false, "b",   false,    ",")]
        [TestCase("ab,",     2,    ",",        "",     false,  false, "ab", false,     true,  false, "",    false,    "")]
        [TestCase("ab,",     3,    ",",        "",     true,   false, "ab", false,     true,  true,  "",    false,    "")]
        [TestCase("ab,",     4,    ",",        "",     true,   false, "ab", false,     true,  true,  "",    false,    "")]

        [TestCase("ab,cd",   1,    ",",        "",     false,  false, "a",  false,     false, false, "b",   false,    ",cd")]
        [TestCase("ab,cd",   2,    ",",        "",     false,  false, "ab", false,     true,  false, "",    false,    "cd")]
        [TestCase("ab,cd",   3,    ",",        "",     true,   false, "ab", false,     false, false, "cd",  false,    "")]
        [TestCase("ab,cd",   4,    ",",        "",     true,   false, "ab", false,     false, false, "c",   false,    "d")]

        [TestCase("ab,cd,e", 1,    ",",        "e",    false,  false, "a",  false,     false, false, "b",   false,    ",cd,e")]
        [TestCase("ab,cd,e", 2,    ",",        "e",    false,  false, "ab", false,     true,  false, "",    false,    "cd,e")]
        [TestCase("ab,cd,e", 3,    ",",        "e",    true,   false, "ab", false,     true,  false, "cd",  false,    "e")]
        [TestCase("ab,cd,e", 4,    ",",        "e",    true,   false, "ab", false,     false, false, "c",   false,    "d,e")]
        
        [TestCase("a\r\nb",  2,    "\r\n",     "",     false,  false, "a",  false,     true,  false, "",    false,    "b")]
        [TestCase("a\r\nb",  3,    "\r\n",     "",     true,   false, "a",  false,     false, false, "b",   false,    "")]
        [TestCase("a\r\nb",  4,    "\r\n",     "",     true,   false, "a",  false,     true,  true,  "b",   false,    "")]
        [TestCase("a\r\nb",  5,    "\r\n",     "",     true,   false, "a",  false,     true,  true,  "b",   false,    "")]
        public void Should_read_to_delimiter_failing_on_invalid_tokens(
            string data, int bufferSize, string delimiter, string invalidTokens,
            bool endOfSection1, bool endOfStream1, string read1, bool invalid1,
            bool endOfSection2, bool endOfStream2, string read2, bool invalid2,
            string remainingData)
        {
            var invalidTokenBytes = invalidTokens.Split(",")
                .Select(x => x.Select(y => (byte)y).ToArray()).ToArray();
            var stream = new MemoryStream(data.ToBytes());
            var buffer = new DelimitedBuffer(stream, bufferSize);
            var readBuffer = new byte[10];

            var result = buffer.ReadTo(readBuffer, 0, 10, delimiter.ToBytes(), invalidTokenBytes);

            result.Invalid.ShouldEqual(invalid1);
            result.EndOfSection.ShouldEqual(endOfSection1);
            result.EndOfStream.ShouldEqual(endOfStream1);
            readBuffer.ToString(result.Read).ShouldEqual(read1);

            result = buffer.ReadTo(readBuffer, 0, 10, delimiter.ToBytes(), invalidTokenBytes);

            result.Invalid.ShouldEqual(invalid2);
            result.EndOfSection.ShouldEqual(endOfSection2);
            result.EndOfStream.ShouldEqual(endOfStream2);
            readBuffer.ToString(result.Read).ShouldEqual(read2);

            ReadRemaining(buffer).ShouldEqual(remainingData);
        }

        //        Data     Buffer  Delimiter   Valid   EOP1   EOS1   Read1 Invalid1   EOP2   EOS2   Read2 Invalid2  Remaining
        [TestCase("",      1,      "",         "",     true,  true,  0,    false,     true,  true,  0,    false,    "")]
        [TestCase(",",     1,      ",",        "b",    true,  false, 0,    false,     true,  true,  0,    false,    "")]

        [TestCase("a",     1,      ",",        "a",    true,  true,  1,    false,     true,  true,  0,    false,    "")]
        [TestCase("a",     2,      ",",        "a",    true,  true,  1,    false,     true,  true,  0,    false,    "")]
        [TestCase("a",     3,      ",",        "a",    true,  true,  1,    false,     true,  true,  0,    false,    "")]
        [TestCase("a",     4,      ",",        "a",    true,  true,  1,    false,     true,  true,  0,    false,    "")]

        [TestCase("a,",    1,      ",",        "a",    true,  false, 1,    false,     true,  true,  0,    false,    "")]
        [TestCase("a,",    2,      ",",        "a",    true,  false, 1,    false,     true,  true,  0,    false,    "")]
        [TestCase("a,",    3,      ",",        "a",    true,  false, 1,    false,     true,  true,  0,    false,    "")]
        [TestCase("a,",    4,      ",",        "a",    true,  false, 1,    false,     true,  true,  0,    false,    "")]

        [TestCase("ab,",   1,      ",",        "a",    false, false, 1,    true,      false, false, 0,    true,     "b,")]
        [TestCase("ab,",   2,      ",",        "a",    false, false, 0,    true,      false, false, 0,    true,     "ab,")]
        [TestCase("ab,",   3,      ",",        "a",    false, false, 0,    true,      false, false, 0,    true,     "ab,")]
        [TestCase("ab,",   4,      ",",        "a",    false, false, 0,    true,      false, false, 0,    true,     "ab,")]

        [TestCase("ab,",   1,      ",",        "b",    false, false, 0,    true,      false, false, 0,    true,     "ab,")]
        [TestCase("ab,",   2,      ",",        "b",    false, false, 0,    true,      false, false, 0,    true,     "ab,")]
        [TestCase("ab,",   3,      ",",        "b",    false, false, 0,    true,      false, false, 0,    true,     "ab,")]
        [TestCase("ab,",   4,      ",",        "b",    false, false, 0,    true,      false, false, 0,    true,     "ab,")]

        [TestCase("ab,",   1,      ",",        "ab",   true,  false, 2,    false,     true,  true,  0,    false,    "")]
        [TestCase("ab,",   2,      ",",        "ab",   true,  false, 2,    false,     true,  true,  0,    false,    "")]
        [TestCase("ab,",   3,      ",",        "ab",   true,  false, 2,    false,     true,  true,  0,    false,    "")]
        [TestCase("ab,",   4,      ",",        "ab",   true,  false, 2,    false,     true,  true,  0,    false,    "")]

        [TestCase("ab,cd",   1,    ",",        "abcd", true,  false, 2,    false,     true,  true,  2,    false,    "")]
        [TestCase("ab,cd",   2,    ",",        "abcd", true,  false, 2,    false,     true,  true,  2,    false,    "")]
        [TestCase("ab,cd",   3,    ",",        "abcd", true,  false, 2,    false,     true,  true,  2,    false,    "")]
        [TestCase("ab,cd",   4,    ",",        "abcd", true,  false, 2,    false,     true,  true,  2,    false,    "")]

        [TestCase("ab,cd,e", 1,    ",",        "abcd", true,  false, 2,    false,     true,  false, 2,    false,    "e")]
        [TestCase("ab,cd,e", 2,    ",",        "abcd", true,  false, 2,    false,     true,  false, 2,    false,    "e")]
        [TestCase("ab,cd,e", 3,    ",",        "abcd", true,  false, 2,    false,     true,  false, 2,    false,    "e")]
        [TestCase("ab,cd,e", 4,    ",",        "abcd", true,  false, 2,    false,     true,  false, 2,    false,    "e")]
        
        [TestCase("a\r\nb",  2,    "\r\n",     "ab",   true,  false, 1,    false,     true,   true, 1,    false,    "")]
        [TestCase("a\r\nb",  3,    "\r\n",     "ab",   true,  false, 1,    false,     true,   true, 1,    false,    "")]
        [TestCase("a\r\nb",  4,    "\r\n",     "ab",   true,  false, 1,    false,     true,   true, 1,    false,    "")]
        [TestCase("a\r\nb",  5,    "\r\n",     "ab",   true,  false, 1,    false,     true,   true, 1,    false,    "")]
        public void Should_read_to_failing_on_invalid_chars(
            string data, int bufferSize, string delimiter, string validChars,
            bool endOfSection1, bool endOfStream1, int read1, bool invalid1,
            bool endOfSection2, bool endOfStream2, int read2, bool invalid2,
            string remainingData)
        {
            var stream = new MemoryStream(data.ToBytes());
            var buffer = new DelimitedBuffer(stream, bufferSize);

            var result = buffer.ReadTo(delimiter.ToBytes(), validChars.ToBytes());

            result.Invalid.ShouldEqual(invalid1);
            result.EndOfSection.ShouldEqual(endOfSection1);
            result.EndOfStream.ShouldEqual(endOfStream1);
            result.Read.ShouldEqual(read1);

            result = buffer.ReadTo(delimiter.ToBytes(), validChars.ToBytes());

            result.Invalid.ShouldEqual(invalid2);
            result.EndOfSection.ShouldEqual(endOfSection2);
            result.EndOfStream.ShouldEqual(endOfStream2);
            result.Read.ShouldEqual(read2);

            ReadRemaining(buffer).ShouldEqual(remainingData);
        }
        
        //        Data     Buffer  Invalid  EOP     EOS    Read  Result Invalid
        [TestCase("",      1,      "",      true,   true,  0,    "",    false)]
        [TestCase("a",     1,      "a",     false,  false, 0,    "",    true)]
        [TestCase("a",     1,      "b",     true,   true,  1,    "a",   false)]

        [TestCase("ab",    1,      "b",     false,  false, 1,    "a",   true)]
        [TestCase("ab",    1,      "c",     true,   true,  2,    "ab",  false)]

        [TestCase("abcde", 2,      "yz,cd", false,  false, 2,    "ab",  true)]

        [TestCase("abcde", 2,      "cd,yz", false,  false, 2,    "ab",  true)]
        [TestCase("abcde", 3,      "cd,yz", false,  false, 2,    "ab",  true)]
        [TestCase("abcde", 4,      "cd,yz", false,  false, 0,    "",    true)]
        [TestCase("abcde", 5,      "cd,yz", false,  false, 0,    "",    true)]
        [TestCase("abcde", 6,      "cd,yz", false,  false, 0,    "",    true)]
        
        [TestCase("abcde", 2,      "ab",    false,  false, 0,    "",    true)]
        [TestCase("abcde", 3,      "ab",    false,  false, 0,    "",    true)]
        [TestCase("abcde", 4,      "ab",    false,  false, 0,    "",    true)]
        [TestCase("abcde", 5,      "ab",    false,  false, 0,    "",    true)]
        [TestCase("abcde", 6,      "ab",    false,  false, 0,    "",    true)]

        [TestCase("abcde", 2,      "de",    false,  false, 3,    "abc", true)]
        [TestCase("abcde", 3,      "de",    false,  false, 2,    "ab",  true)]
        [TestCase("abcde", 4,      "de",    false,  false, 3,    "abc", true)]
        [TestCase("abcde", 5,      "de",    false,  false, 0,    "",    true)]
        [TestCase("abcde", 6,      "de",    false,  false, 0,    "",    true)]
        
        [TestCase("abcde", 6,      "xy",    true,   true,  5,  "abcde", false)]
        public void Should_read_failing_on_invalid_tokens(
            string data, int bufferSize, string invalidTokens, bool endOfSection, 
            bool endOfStream, int read, string expectedData, bool invalid)
        {
            var invalidTokenBytes = invalidTokens.Split(",")
                .Select(x => x.Select(y => (byte)y).ToArray()).ToArray();
            var stream = new MemoryStream(data.ToBytes());
            var buffer = new DelimitedBuffer(stream, bufferSize);
            var readBuffer = new byte[10];
            var readData = "";
            var readLength = 0;
            DelimitedBuffer.ReadResult result;

            while (true)
            {
                result = buffer.Read(readBuffer, 0, 10, invalidTokenBytes);
                readLength += result.Read;
                if (result.Read > 0) readData += readBuffer.ToString(result.Read);
                if (result.Invalid || result.EndOfSection || result.EndOfStream) break;
            }
            
            result.Invalid.ShouldEqual(invalid);
            result.EndOfSection.ShouldEqual(endOfSection);
            result.EndOfStream.ShouldEqual(endOfStream);
            readLength.ShouldEqual(read);
            readData.ShouldEqual(expectedData);
        }

        private string ReadRemaining(DelimitedBuffer delimitedBuffer)
        {
            var buffer = new byte[10];
            var data = "";

            while (true)
            {
                var result = delimitedBuffer.Read(buffer, 0, 10);
                if (result.Read > 0) data += buffer.ToString(result.Read);
                if (result.Invalid || result.EndOfSection || result.EndOfStream) break;
            }

            return data;
        }

        [Test]
        public void Should_fail_if_buffer_less_than_1()
        {
            Assert.Throws<ArgumentException>(() => new DelimitedBuffer(null, 0));
        }

        [Test]
        public void Should_indicate_when_at_begining_of_stream()
        {
            var stream = new MemoryStream("fark".ToBytes());
            var buffer = new DelimitedBuffer(stream);

            buffer.BeginingOfStream.ShouldBeTrue();

            buffer.ReadTo(null, 0, 2, null);

            buffer.BeginingOfStream.ShouldBeFalse();
        }

        [Test]
        public void Should_fail_if_offset_less_than_0()
        {
            new DelimitedBuffer(new MemoryStream())
                .Should().Throw<ArgumentException>(
                    x => x.ReadTo(new byte[] { }, -1, 1, "fark".ToBytes()));
        }

        [Test]
        public void Should_fail_if_count_less_than_0()
        {
            new DelimitedBuffer(new MemoryStream())
                .Should().Throw<ArgumentException>(x =>
                    x.ReadTo(new byte[] { }, 0, -1, "fark".ToBytes()));
        }

        [Test]
        public void Should_fail_if_buffer_size_less_than_minimum_padding()
        {
            new DelimitedBuffer(new MemoryStream(), 5)
                .Should().Throw<ArgumentException>(x =>
                    x.ReadTo(new byte[] { }, 0, 1, "fark".ToBytes(), "farker".ToBytes()));
        }
    }
}
