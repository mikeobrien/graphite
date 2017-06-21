using System;
using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Writers;

namespace Tests.Common.Fakes
{
    public class TestResponseWriter1 : TestResponseWriter { }
    public class TestResponseWriter2 : TestResponseWriter { }

    public class TestResponseWriter : IResponseWriter
    {
        public Func<ResponseWriterContext, bool> AppliesToFunc { get; set; }
        public Func<ResponseWriterContext, Task<HttpResponseMessage>> WriteFunc { get; set; }
        public ResponseWriterContext AppliesToContext { get; set; }
        public ResponseWriterContext WriteContext { get; set; }
        public bool AppliesToCalled { get; set; }
        public bool WriteCalled { get; set; }

        public bool IsWeighted { get; set; }
        public double Weight { get; set; }

        public bool AppliesTo(ResponseWriterContext context)
        {
            AppliesToCalled = true;
            AppliesToContext = context;
            return AppliesToFunc?.Invoke(context) ?? true;
        }


        public Task<HttpResponseMessage> Write(ResponseWriterContext context)
        {
            WriteCalled = true;
            WriteContext = context;
            return WriteFunc(context);
        }
    }
}
