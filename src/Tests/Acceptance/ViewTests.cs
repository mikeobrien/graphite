using System.Net;
using Graphite.Http;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Acceptance
{
    [TestFixture]
    public class ViewTests
    {
        private const string BaseUrl = "Views/";

        [Test]
        public void Should_return_first_view_when_multiple_apply([Values(Host.Owin, Host.IISExpress)] Host host)
        {
            var result = Http.ForHost(host).GetHtml($"{BaseUrl}multiple");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.ContentType.ShouldEqual(MimeTypes.TextHtml);
            result.Data.ShouldEqual("fark");
        }
        
        [Test]
        public void Should_return_view(
            [Values(Host.Owin, Host.IISExpress)] Host host,
            [Values("file", "resource")] string type,
            [Values("mustache", "razor")] string engine)
        {
            var result = Http.ForHost(host)
                .GetHtml($"{BaseUrl}{engine}/{type}");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.ContentType.ShouldEqual(MimeTypes.TextHtml);
            result.Data.ShouldEqual($"{engine} {type} fark");
        }
    }
}
