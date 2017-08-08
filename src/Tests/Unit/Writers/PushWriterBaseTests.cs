using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Graphite;
using Graphite.Http;
using Graphite.Writers;
using NSubstitute;
using NUnit.Framework;
using Should;

namespace Tests.Unit.Writers
{
    [TestFixture]
    public class PushWriterBaseTests
    {
        public class DummyPushWriter : PushWriterBase
        {
            public DummyPushWriter(HttpRequestMessage requestMessage, 
                HttpResponseMessage responseMessage, Configuration configuration, 
                params string[] mimeTypes) : 
                base(requestMessage, responseMessage, configuration, mimeTypes) { }

            protected override Task WriteResponse(ResponseWriterContext context, 
                Stream stream, TransportContext transportContext)
            {
                var writer = new System.IO.StreamWriter(stream);
                writer.Write(context.Response.ToString());
                writer.Flush();
                return Task.CompletedTask;
            }
        }

        private Configuration _configuration;
        private HttpRequestMessage _requestMessage;
        private HttpResponseMessage _responseMessage;
        private WeightedContentWriterBase _writer;

        [SetUp]
        public void Setup()
        {
            _configuration = new Configuration();
            _requestMessage = new HttpRequestMessage();
            _responseMessage = new HttpResponseMessage();
            _writer = new DummyPushWriter(_requestMessage,
                _responseMessage, _configuration, 
                MimeTypes.ApplicationJson, MimeTypes.TextPlain);
        }

        [TestCase("*/*", MimeTypes.ApplicationJson)]
        [TestCase("application/*", MimeTypes.ApplicationJson)]
        [TestCase("text/*", MimeTypes.TextPlain)]
        [TestCase(MimeTypes.ApplicationJson, MimeTypes.ApplicationJson)]
        [TestCase(MimeTypes.TextPlain, MimeTypes.TextPlain)]
        public void Should_push_content(string acceptType, string contentType)
        {
            _requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptType));
            
            var result = _writer.Write(new ResponseWriterContext("fark")).Result;
            var content = result.Content.ReadAsStringAsync().Result;

            result.Content.Headers.ContentType.MediaType.ShouldEqual(contentType);
            content.ShouldEqual("fark");
        }

        [Test]
        public void Should_dispose_response_if_disposable(
            [Values(true, false)] bool dispose)
        {
            _configuration.DisposeResponses = dispose;
            _requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(
                MimeTypes.ApplicationJson));

            var disposable = Substitute.For<IDisposable>();

            _writer.Write(new ResponseWriterContext(disposable));

            if (dispose)
                disposable.ReceivedWithAnyArgs();
            else
                disposable.DidNotReceiveWithAnyArgs();
        }
    }
}
