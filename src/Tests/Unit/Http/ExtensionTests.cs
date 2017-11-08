using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Graphite.Http;
using Graphite.Linq;
using Graphite.Extensions;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Http
{
    [TestFixture]
    public class ExtensionTests
    {
        [TestCase(null, null)]
        [TestCase("", "")]
        [TestCase("\"", "\"")]
        [TestCase("\"\"", "")]
        [TestCase("\"fark\"", "fark")]
        [TestCase("   \"fark\"\t\t", "fark")]
        public void Should_unquote_string(
            string value, string expected)
        {
            value.Unquote().ShouldEqual(expected);
        }

        [TestCase(null, "", "")]
        [TestCase("", "", "")]
        [TestCase("fark", "fark", "")]
        [TestCase("fark:", "fark", "")]
        [TestCase("fark: farker", "fark", "farker")]
        [TestCase("fark: farker:farkiest ", "fark", "farker:farkiest")]
        public void Should_parse_header(
            string header, string name, string value)
        {
            var result = header.ParseHeader();
            result.Key.ShouldEqual(name);
            result.Value.ShouldEqual(value);
        }

        [TestCase(null, null)]
        [TestCase("", null)]
        [TestCase("fark", "fark:")]
        [TestCase("fark : farker", "fark:farker")]
        [TestCase("fark : farker\r\n", "fark:farker")]
        [TestCase("fark : farker\r\noh", "fark:farker,oh:")]
        [TestCase("fark : farker\r\noh: hai", "fark:farker,oh:hai")]
        public void Should_parse_headers(string headers, string expected)
        {
            var result = headers.ParseHeaders();

            if (expected.IsNullOrEmpty())
            {
                result.ShouldBeEmpty();
                return;
            }

            var compare = expected.Split(',').Select(x => x.Split(':')).ToArray();

            result.Count.ShouldEqual(compare.Length);

            for (var i = 0; i < compare.Length; i++)
            {
                result[i].Key.ShouldEqual(compare[i][0]);
                result[i].Value.ShouldEqual(compare[i][1]);
            }
        }

        [TestCase(null, null)]
        [TestCase("", null)]
        [TestCase(" fark ", "fark")]
        [TestCase(" fark , farker ", "fark,farker")]
        public void Should_parse_tokens(string tokens, string expected)
        {
            if (expected == null)
                tokens.ParseTokens().ShouldBeNull();
            else tokens.ParseTokens().ShouldOnlyContain(expected.Split(','));
        }

        [Test]
        public void Should_return_boundry([Values("", "\"")] string qualifer)
        {
            var content = new StringContent("");

            var contentType = content.Headers.ContentType = new MediaTypeHeaderValue("fark/farker");
            contentType.Parameters.Add(new NameValueHeaderValue(
                "boundary", $"{qualifer}fark-boundary{qualifer}"));

            content.Headers.GetContentBoundry().ShouldEqual("fark-boundary");
        }
		
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
