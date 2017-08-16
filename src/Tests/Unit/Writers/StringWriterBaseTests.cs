using System;
using System.Net.Http;
using System.Text;
using Graphite;
using Graphite.Writers;
using NUnit.Framework;
using Should;

namespace Tests.Unit.Writers
{
    [TestFixture]
    public class StringWriterBaseTests
    {
        [TestCase("application/json,application/*;q=.9,*/*", 1)]
        [TestCase("application/json;q=.9,application/*,*/*", .9999)]
        [TestCase("text/json,text/*,*/*", .9998)]
        [TestCase("text/json,text/*,*/*;q=.5", .4998)]
        public void Should_return_weight(string acceptHeader, double weight)
        {
            var writer = new TestWriter(acceptHeader, "application/json");

            writer.Weight.ShouldEqual(weight);
        }

        public class TestWriter : StringWriterBase
        {
            public TestWriter(string acceptHeader, params string[] mimeTypes) : 
                base(CreateRequest(acceptHeader), new HttpResponseMessage(), 
                    Encoding.UTF8, new Configuration(), mimeTypes) { }

            private static HttpRequestMessage CreateRequest(string acceptHeader)
            {
                var request = new HttpRequestMessage();
                request.Headers.Accept.ParseAdd(acceptHeader);
                return request;
            }

            protected override string GetResponse(ResponseWriterContext context)
            {
                throw new NotImplementedException();
            }
        }
    }
}
