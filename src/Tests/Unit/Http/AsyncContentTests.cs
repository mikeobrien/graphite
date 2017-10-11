using System.IO;
using System.Text;
using Graphite.Http;
using NUnit.Framework;
using Should;

namespace Tests.Unit.Http
{
    [TestFixture]
    public class AsyncContentTests
    {
        [Test]
        public void Should_close_source_stream()
        {
            var source = new MemoryStream(Encoding.UTF8.GetBytes("fark"));
            var content = new AsyncContent(source, 1024);

            content.ReadAsStringAsync().Result.ShouldEqual("fark");

            source.CanRead.ShouldBeFalse();
        }
    }
}
