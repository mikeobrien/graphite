using Graphite.Extensions;
using NUnit.Framework;
using Should;

namespace Tests.Unit.Extensions
{
    [TestFixture]
    public class NumericExtensionTests
    {
        [Test]
        public void Should_return_byte_string()
        {
            0.ToSizeString().ShouldEqual("0B");
            1023.ToSizeString().ShouldEqual("1023B");
        }

        [Test]
        public void Should_return_kbyte_string()
        {
            1024.ToSizeString().ShouldEqual("1KB");
            1500.ToSizeString().ShouldEqual("1.5KB");
            (1024 * 1024 - 1).ToSizeString().ShouldEqual("1024KB");
        }

        [Test]
        public void Should_return_mbyte_string()
        {
            (1024 * 1024).ToSizeString().ShouldEqual("1MB");
            (1500 * 1024).ToSizeString().ShouldEqual("1.5MB");
            (1024 * 1024 * 1024 - 1).ToSizeString().ShouldEqual("1024MB");
        }

        [Test]
        public void Should_return_gbyte_string()
        {
            (1024 * 1024 * 1024).ToSizeString().ShouldEqual("1GB");
            (1500 * 1024 * 1024).ToSizeString().ShouldEqual("1.5GB");
        }
    }
}
