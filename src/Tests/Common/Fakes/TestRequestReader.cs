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
            ReadFunc = () => Task.FromResult(ReadResult.Success(null));
        }

        public Func<bool> AppliesFunc { get; set; }
        public Func<Task<ReadResult>> ReadFunc { get; set; }
        public bool AppliesCalled { get; set; }
        public bool ReadCalled { get; set; }

        public bool Applies()
        {
            AppliesCalled = true;
            return AppliesFunc?.Invoke() ?? true;
        }

        public Task<ReadResult> Read()
        {
            ReadCalled = true;
            return ReadFunc();
        }
    }
}
