using System.Net;
using System.Net.Http;
using System.Threading;
using Graphite.Http;
using NUnit.Framework;
using Should;

namespace Tests.Unit.Http
{
    [TestFixture]
    public class TextResultTests
    {
        [Test]
        public void Should_create_response([Values("\r", "\n", "\r\n")] string reasonPhraseWhitespace)
        {
            var response = new TextResult(new HttpRequestMessage(), "data", 
                HttpStatusCode.NotFound, $"status{reasonPhraseWhitespace}text")
                .ExecuteAsync(new CancellationToken()).Result;

            response.StatusCode.ShouldEqual(HttpStatusCode.NotFound);
            response.ReasonPhrase.ShouldEqual("status text");
            response.Content.ReadAsStringAsync().Result.ShouldEqual("data");
            response.Content.Headers.ContentType.MediaType.ShouldEqual(MimeTypes.TextPlain);
        }
    }
}
