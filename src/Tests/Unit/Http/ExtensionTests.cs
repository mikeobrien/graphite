using System.Net.Http;
using Graphite.Extensions;
using Graphite.Http;
using NUnit.Framework;
using Should;

namespace Tests.Unit.Http
{
    [TestFixture]
    public class ExtensionTests
    {
        [TestCase(null, "")]
        [TestCase("", "")]
        [TestCase("aa", "aa")]
        [TestCase("aaaaaaaa", "aaaaaaa")]
        [TestCase("a\r\na\ra\naaaaa", "a a a a")]
        public void Should_set_valid_reason_phrase(string reasonPhrase, string expected)
        {
            var response = new HttpResponseMessage();

            var padding = reasonPhrase.IsNotNullOrEmpty() ? new string('a', 505) : "";

            response.SafeSetReasonPhrase(padding + reasonPhrase);

            response.ReasonPhrase.ShouldEqual(padding + expected);
        }
    }
}
