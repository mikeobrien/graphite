using System;
using System.Threading.Tasks;
using Graphite.Binding;

namespace Tests.Common.Fakes
{
    public class TestRequestBinder1 : TestRequestBinder { }
    public class TestRequestBinder2 : TestRequestBinder { }

    public class TestRequestBinder : IRequestBinder
    {
        public TestRequestBinder()
        {
            BindFunc = c => Task.FromResult<object>(null);
        }

        public Func<RequestBinderContext, bool> AppliesToFunc { get; set; }
        public Func<RequestBinderContext, Task> BindFunc { get; set; }
        public RequestBinderContext AppliesToContext { get; set; }
        public RequestBinderContext BindContext { get; set; }
        public bool AppliesToCalled { get; set; }
        public bool BindCalled { get; set; }

        public bool AppliesTo(RequestBinderContext context)
        {
            AppliesToCalled = true;
            AppliesToContext = context;
            return AppliesToFunc?.Invoke(context) ?? true;
        }

        public Task Bind(RequestBinderContext context)
        {
            BindCalled = true;
            BindContext = context;
            return BindFunc(context);
        }
    }
}
