using Graphite.Http;
using NUnit.Framework;
using Should;

namespace Tests.Unit.Http
{
    [TestFixture]
    public class HttpMethodTests
    {
        [Test]
        public void Should_mod_http_method_regex()
        {
            var methods = new HttpMethods
            {
                HttpMethod.Connect, HttpMethod.Delete
            };

            methods.WithRegex(x => $"^{x.Method}");

            methods[0].ActionRegex.ShouldEqual("^CONNECT");
            methods[1].ActionRegex.ShouldEqual("^DELETE");
        }
    }
}
