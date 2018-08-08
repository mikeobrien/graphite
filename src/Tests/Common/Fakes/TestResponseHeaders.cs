using System;
using Graphite.Actions;

namespace Tests.Common.Fakes
{
    public class TestResponseHeaders1 : TestResponseHeaders { }
    public class TestResponseHeaders2 : TestResponseHeaders { }

    public class TestResponseHeaders : IResponseHeaders
    {
        public Func<ResponseHeadersContext, bool> AppliesToFunc { get; set; }
        public Action<ResponseHeadersContext> SetHeadersFunc { get; set; }
        public ResponseHeadersContext AppliesToContext { get; set; }
        public ResponseHeadersContext SetHeadersContext { get; set; }
        public bool AppliesToCalled { get; set; }
        public bool SetHeadersCalled { get; set; }

        public bool AppliesTo(ResponseHeadersContext context)
        {
            AppliesToCalled = true;
            AppliesToContext = context;
            return AppliesToFunc?.Invoke(context) ?? true;
        }
        
        public void SetHeaders(ResponseHeadersContext context)
        {
            SetHeadersCalled = true;
            SetHeadersContext = context;
            SetHeadersFunc(context);
        }
    }
}