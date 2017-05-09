using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Graphite.Extensions;
using NUnit.Framework;
using Should;

namespace Tests.Unit.Extensions
{
    [TestFixture]
    public class WebApiExtensionTests
    {
        [TestCase("text/plain", "TEXT/PLAIN", true)]
        [TestCase("text/plain", "text/html", false)]
        [TestCase("text/plain,*/*", "text/html", true)]
        [TestCase("application/xml,text/*", "text/html", true)]
        public void Should_indicate_if_accept_header_contains_mime_type(
            string acceptTypes, string compare, bool isMatch)
        {
            var requestMessage = new HttpRequestMessage
            {
                Content = new HttpMessageContent(new HttpRequestMessage())
            };

            requestMessage.Headers.Add("Accept", acceptTypes);

            requestMessage.AcceptsMimeType(compare).ShouldEqual(isMatch);
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
    }
}
