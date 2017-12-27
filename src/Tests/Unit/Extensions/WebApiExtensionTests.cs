using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Graphite.Extensions;
using NUnit.Framework;
using Should;

namespace Tests.Unit.Extensions
{
    [TestFixture]
    public class WebApiExtensionTests
    {
        [TestCase("text/plain,text/html,text/*,*/*", 
            MatchType.Full, "text/plain", 1,
            MatchType.Partial, "text/*", 1,
            MatchType.Any, "*/*", 1)]
        [TestCase("text/plain;q=.5,text/html,text/*,*/*",
            MatchType.Partial, "text/*", 1,
            MatchType.Any, "*/*", 1,
            MatchType.Full, "text/plain", .5)]
        public void Should_get_accept_types_in_quality_order(
            string acceptHeader,
            MatchType matchType1, string acceptType1, double type1Quality,
            MatchType matchType2, string acceptType2, double type2Quality,
            MatchType matchType3, string acceptType3, double type3Quality)
        {
            var requestMessage = new HttpRequestMessage
            {
                Content = new HttpMessageContent(new HttpRequestMessage())
            };

            requestMessage.Headers.Add("Accept", acceptHeader);

            var results = requestMessage.GetMatchingAcceptTypes("text/plain");

            results.Count.ShouldEqual(3);

            var result = results[0];

            result.ContentType.ShouldEqual("text/plain");
            result.AcceptType.ShouldEqual(acceptType1);
            result.Quality.ShouldEqual(type1Quality);
            result.MatchType.ShouldEqual(matchType1);

            result = results[1];

            result.ContentType.ShouldEqual("text/plain");
            result.AcceptType.ShouldEqual(acceptType2);
            result.Quality.ShouldEqual(type2Quality);
            result.MatchType.ShouldEqual(matchType2);

            result = results[2];

            result.ContentType.ShouldEqual("text/plain");
            result.AcceptType.ShouldEqual(acceptType3);
            result.Quality.ShouldEqual(type3Quality);
            result.MatchType.ShouldEqual(matchType3);
        }

        [TestCase(null, null, MatchType.None)]
        [TestCase("", "", MatchType.None)]
        [TestCase("text/plain", "text/html", MatchType.None)]
        [TestCase("text/plain", "TEXT/PLAIN", MatchType.Full)]
        [TestCase("text/plain", "TEXT/*", MatchType.Partial)]
        [TestCase("text/plain", "*/*", MatchType.Any)]
        public void Should_determine_accept_type_match(string contentType, 
            string acceptType, MatchType matchType)
        {
            contentType.MatchesAcceptType(acceptType).ShouldEqual(matchType);
        }

        [TestCase("text/plain", "TEXT/PLAIN", true)]
        [TestCase("text/plain", "text/html", false)]
        public void Should_determine_content_type(string contentType, string compare, bool isMatch)
        {
            new HttpRequestMessage
            {
                Content = new HttpMessageContent(new HttpRequestMessage())
                {
                    Headers = { ContentType = new MediaTypeHeaderValue(contentType) }
                }
            }.ContentTypeIs(compare).ShouldEqual(isMatch);
        }

        [TestCase(MatchType.None, 1.0)]
        [TestCase(MatchType.Full, 1.0)]
        [TestCase(MatchType.Partial, .9999)]
        [TestCase(MatchType.Any, .9998)]
        public void Should_weight_quailty_by_on_match_type(MatchType matchType, double expected)
        {
            new AcceptTypeMatch(matchType, null, null, 1).GetWeight().ShouldEqual(expected);
        }

        [Test]
        public void Should_rebuild_raw_request()
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, "http://fark/farker");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("fark/farker"));

            request.RawHeaders().ShouldEqual(
                "DELETE /farker HTTP/1.1\r\n" +
                "Accept: fark/farker");
        }

        [Test]
        public void Should_rebuild_raw_request_with_content_headers()
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, "http://fark/farker");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("fark/farker"));
            request.Content = new StringContent("fark", Encoding.UTF8, "oh/hai");

            request.RawHeaders().ShouldEqual(
                "DELETE /farker HTTP/1.1\r\n" +
                "Accept: fark/farker\r\n" +
                "Content-Type: oh/hai; charset=utf-8");
        }

        [TestCase("fark.text", "fark.text")]
        [TestCase("fark farker.txt", "\"fark farker.txt\"")]
        [TestCase("fark \"farker\".txt", "\"fark farker.txt\"")]
        public void Should_set_attachment_disposition(string filename, string expected)
        {
            var response = new HttpResponseMessage { Content = new StringContent("") };

            response.Content.Headers.SetAttachmentDisposition(filename);

            response.Content.Headers.ContentDisposition.FileName.ShouldEqual(expected);
        }
    }
}
