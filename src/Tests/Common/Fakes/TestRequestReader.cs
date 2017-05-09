using System;
using System.Threading.Tasks;
using Graphite.Readers;

namespace Tests.Common.Fakes
{
    public class TestRequestReader1 : TestRequestReader { }
    public class TestRequestReader2 : TestRequestReader { }

    public class TestRequestReader : IRequestReader
    {
        public TestRequestReader()
        {
            ReadFunc = c => Task.FromResult<object>(null);
        }

        public Func<RequestReaderContext, bool> AppliesToFunc { get; set; }
        public Func<RequestReaderContext, Task<object>> ReadFunc { get; set; }
        public RequestReaderContext AppliesToContext { get; set; }
        public RequestReaderContext ReadContext { get; set; }
        public bool AppliesToCalled { get; set; }
        public bool ReadCalled { get; set; }

        public bool AppliesTo(RequestReaderContext context)
        {
            AppliesToCalled = true;
            AppliesToContext = context;
            return AppliesToFunc?.Invoke(context) ?? true;
        }

        public Task<object> Read(RequestReaderContext context)
        {
            ReadCalled = true;
            ReadContext = context;
            return ReadFunc(context);
        }
    }
}
