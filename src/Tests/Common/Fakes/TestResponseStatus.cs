using System;
using Graphite.Actions;

namespace Tests.Common.Fakes
{
    public class TestResponseStatus1 : TestResponseStatus { }
    public class TestResponseStatus2 : TestResponseStatus { }

    public class TestResponseStatus : IResponseStatus
    {
        public Func<ResponseStatusContext, bool> AppliesToFunc { get; set; }
        public Action<ResponseStatusContext> SetStatusFunc { get; set; }
        public ResponseStatusContext AppliesToContext { get; set; }
        public ResponseStatusContext SetStatusContext { get; set; }
        public bool AppliesToCalled { get; set; }
        public bool SetStatusCalled { get; set; }

        public bool IsWeighted { get; set; }
        public double Weight { get; set; }

        public bool AppliesTo(ResponseStatusContext context)
        {
            AppliesToCalled = true;
            AppliesToContext = context;
            return AppliesToFunc?.Invoke(context) ?? true;
        }


        public void SetStatus(ResponseStatusContext context)
        {
            SetStatusCalled = true;
            SetStatusContext = context;
            SetStatusFunc(context);
        }
    }
}
