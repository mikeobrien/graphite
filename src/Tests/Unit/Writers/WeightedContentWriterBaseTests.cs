using System.Net.Http;
using System.Net.Http.Headers;
using Graphite.Http;
using Graphite.Writers;
using NUnit.Framework;
using Should;

namespace Tests.Unit.Writers
{
    [TestFixture]
    public class WeightedContentWriterBaseTests
    {
        public class DummyContentWriter : WeightedContentWriterBase
        {
            public DummyContentWriter(HttpRequestMessage requestMessage, 
                HttpResponseMessage responseMessage, params string[] mimeTypes) : 
                base(requestMessage, responseMessage, mimeTypes) { }

            protected override HttpContent GetContent(ResponseWriterContext context)
            {
                return new StringContent(context.Response.ToString());
            }
        }

        private HttpRequestMessage _requestMessage;
        private HttpResponseMessage _responseMessage;
        private WeightedContentWriterBase _writer;

        [SetUp]
        public void Setup()
        {
            _requestMessage = new HttpRequestMessage();
            _responseMessage = new HttpResponseMessage();
            _writer = new DummyContentWriter(_requestMessage, 
                _responseMessage, MimeTypes.ApplicationJson, 
                MimeTypes.TextPlain);
        }

        [TestCase(MimeTypes.ApplicationJson, true)]
        [TestCase(MimeTypes.TextPlain, true)]
        [TestCase(MimeTypes.TextCsv, false)]
        [TestCase("application/*", true)]
        [TestCase("*/*", true)]
        public void Should_apply_when_accept_matches(string acceptType, bool applies)
        {
            _requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptType));

            _writer.AppliesTo(new ResponseWriterContext("")).ShouldEqual(applies);
        }

        [TestCase(MimeTypes.ApplicationJson, null, 1)]
        [TestCase(MimeTypes.ApplicationJson, .2, .2)]
        [TestCase("application/*", null, 0.9999)]
        [TestCase("*/*", null, 0.9998)]
        public void Should_be_weighted(string acceptType, double? quality, double weight)
        {
            _requestMessage.Headers.Accept.Add(quality.HasValue 
                ? new MediaTypeWithQualityHeaderValue(acceptType, quality.Value)
                : new MediaTypeWithQualityHeaderValue(acceptType));
            _writer.IsWeighted.ShouldBeTrue();
            _writer.Weight.ShouldEqual(weight);
        }
        
        [TestCase("*/*", MimeTypes.ApplicationJson)]
        [TestCase("application/*", MimeTypes.ApplicationJson)]
        [TestCase("text/*", MimeTypes.TextPlain)]
        [TestCase(MimeTypes.ApplicationJson, MimeTypes.ApplicationJson)]
        [TestCase(MimeTypes.TextPlain, MimeTypes.TextPlain)]
        public void Should_write_content(string acceptType, string contentType)
        {
            _requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptType));

            var result = _writer.Write(new ResponseWriterContext("fark")).Result;
            var content = result.Content.ReadAsStringAsync().Result;
            
            result.Content.Headers.ContentType.MediaType.ShouldEqual(contentType);
            content.ShouldEqual("fark");
        }
    }
}
