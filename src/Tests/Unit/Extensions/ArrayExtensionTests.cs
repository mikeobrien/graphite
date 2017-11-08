using System.Text;
using Graphite.Extensions;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Extensions
{
    [TestFixture]
    public class ArrayExtensionTests
    {
        [TestCase(null, "fark", 0, false, Description = "Null source.")]
        [TestCase("fark", null, 0, false, Description = "Null compare.")]
        [TestCase("", "fark", 0, false, Description = "Empty source.")]
        [TestCase("fark", "", 0, false, Description = "Empty compare.")]
        [TestCase("fark", "fark", -1, false, Description = "Negative offset.")]
        [TestCase("fark", "farker", 0, false, Description = "Compare larger than source.")]
        [TestCase("fark", "fark", 0, true, Description = "Compare same size as source.")]
        [TestCase("fark", "ark", 1, true, Description = "Offset, same size.")]
        [TestCase("fark", "arker", 1, false, Description = "Offset, larger.")]
        [TestCase("fark", "k", 3, true, Description = "Offset, last byte of source.")]
        public void Should_contain_sequence_at(string value,
            string match, int offset, bool startsWith)
        {
            value.ToBytes()
                .ContainsSequenceAt(match.ToBytes(), offset)
                .ShouldEqual(startsWith);
        }

        [TestCase("a", "b", 0, 0, true)]
        [TestCase("a", "b", -1, 1, true)]
        [TestCase(null, "a", 0, 1, true)]
        [TestCase("", "a", 0, 1, true)]
        [TestCase("a", null, 0, 1, true)]
        [TestCase("a", "", 0, 1, true)]
        [TestCase("a", "b", 2, 3, true)]
        [TestCase("ab", "b", 1, 5, true)]
        [TestCase("ab", "b", 2, 5, true)]
        [TestCase("ab", "b", 0, 5, false)]
        [TestCase("abcdef", "cd", 2, 2, true)]
        [TestCase("abcdef", "cd", 2, 3, false)]
        public void Should_only_contain_bytes(string data, string validChars,
            int offset, int length, bool expected)
        {
            data.ToBytes().OnlyContains(validChars?.ToBytes(), offset, length)
                .ShouldEqual(expected);
        }

        [TestCase(null, "abcdef", 0, 1, -1, Description = "Null source.")]
        [TestCase("abcdef", null, 0, 1, -1, Description = "Null compare.")]
        [TestCase("", "abcdef", 0, 1, -1, Description = "Empty source.")]
        [TestCase("abcdef", "", 0, 1, -1, Description = "Empty compare.")]
        [TestCase("abcdef", "abcdef", -1, 0, -1, Description = "Negative offset.")]
        [TestCase("abcdef", "abcdef", 0, -1, -1, Description = "Negative length.")]
        [TestCase("abcdef", "abcdef", 0, 0, -1, Description = "Zero length.")]
        [TestCase("abcdef", "abcdef", 0, 1, 0, Description = "Compare from start, compare same as source, larger than window.")]
        [TestCase("abcdef", "abcdef", 0, 10, 0, Description = "Compare from start, source, compare, window same size.")]
        [TestCase("abcdef", "abcdefg", 0, 10, -1, Description = "Compare from start, compare larger than source.")]
        [TestCase("abcdef", "cd", 1, 2, 1, Description = "Compare from offset, last byte of window, first byte of compare.")]
        [TestCase("abcdef", "cd", 2, 2, 0, Description = "Compare from offset, compare same size as window.")]
        [TestCase("abcdef", "cd", 3, 2, -1, Description = "Compare from offset, window one byte after match.")]
        [TestCase("abcdef", "bcdefg", 1, 10, -1, Description = "Compare from offset, compare larger than source.")]
        [TestCase("abcdef", "f", 4, 2, 1, Description = "Compare from offset, compare last byte of buffer.")]
        public void Should_find_sequence_in_range(string value,
            string find, int offset, int length, int index)
        {
            var findBytes = find.ToBytes();
            var result = value.ToBytes()
                .IndexOfSequence(findBytes, offset, length);

            result.ShouldEqual(index);
        }

        [Test]
        public void Should_copy_array()
        {
            var source = "fark".ToBytes();
            source.Copy(4).ShouldOnlyContain(source);
        }

        [TestCase(null, "fark", "fark")]
        [TestCase("farker", "fark", "farker")]
        public void Should_ensure_array_value(string existing, string @default, string expected)
        {
            var values = new [] { existing };
            var result = values.EnsureValue(0, () => @default);

            result.ShouldEqual(expected);
            values[0].ShouldEqual(expected);
        }

        [Test]
        public void Should_convert_byte_array_to_string()
        {
            Encoding.UTF8.GetBytes("fark").ToString(4).ShouldEqual("fark");
        }

        [Test]
        public void Should_convert_string_to_byte_array()
        {
            "fark".ToBytes().ShouldOnlyContain(Encoding.UTF8.GetBytes("fark"));
        }
    }
}
