using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Graphite;
using Graphite.Http;
using Graphite.Writers;
using NSubstitute;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Writers
{
    [TestFixture]
    public class SerializerWriterBaseTests
    {
        public class FakeWriter : SerializerWriterBase
        {
            public FakeWriter(HttpRequestMessage requestMessage,
                Configuration configuration) : 
                base(requestMessage, new HttpResponseMessage(), 
                    configuration, MimeTypes.TextHtml) { }

            public Action<ResponseWriterContext, Stream> WriteAction { get; set; }

            protected override void WriteToStream(ResponseWriterContext context, Stream output)
            {
                WriteAction(context, output);
            }
        }

        private Configuration _configuration;
        private FakeWriter _writer;

        [SetUp]
        public void Setup()
        {
            var requestMessage = new HttpRequestMessage();
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MimeTypes.TextHtml));
            _configuration = new Configuration();
            _writer = new FakeWriter(requestMessage, _configuration);
        }

        [Test]
        public void Should_return_content()
        {
            _writer.WriteAction = (c, o) => o.WriteAllText(c.Response.ToString());

            var response = _writer.Write(new ResponseWriterContext("fark")).Result;

            response.Content.ShouldNotBeNull();
            response.Content.ReadAsStringAsync().Result.ShouldEqual("fark");
        }

        [Test]
        public void Should_dispose_response_object(
            [Values(true, false)] bool dispose,
            [Values(true, false)] bool exception)
        {
            _configuration.DisposeSerializedObjects = dispose;

            var disposable = Substitute.For<IDisposable>();

            _writer.WriteAction = (c, o) =>
            {
                if (exception) throw new Exception();
            };

            void Write() => _writer
                .Write(new ResponseWriterContext(disposable))
                .Result.Content.ReadAsStringAsync().Wait();

            if (exception) Assert.Throws<Exception>(() => Write());
            else Write();

            if (dispose) disposable.ReceivedWithAnyArgs().Dispose();
            else disposable.DidNotReceiveWithAnyArgs().Dispose();
        }
    }
}

