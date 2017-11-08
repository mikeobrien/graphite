using System.IO;
using Graphite.Extensions;
using NUnit.Framework;
using Should;

namespace Tests.Unit.Extensions
{
    [TestFixture]
    public class IOExtensionTests
    {
        [Test]
        public void Should_read_stream_as_byte_array()
        {
            new MemoryStream("fark".ToBytes()).ToTaskResult<Stream>()
                .ReadAsByteArray().Result.ToString(4).ShouldEqual("fark");
        }

        [Test]
        public void Should_read_stream_as_string()
        {
            new MemoryStream("fark".ToBytes()).ToTaskResult<Stream>()
                .ReadAsString().Result.ShouldEqual("fark");
        }
    }
}
