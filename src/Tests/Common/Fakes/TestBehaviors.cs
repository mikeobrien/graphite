using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Behaviors;
using Graphite.Extensions;

namespace Tests.Common.Fakes
{
    public class TestBehavior1 : TestBehavior { }
    public class TestBehavior2 : TestBehavior { }

    public class TestBehavior : IBehavior
    {
        public bool ShouldRunFlag { get; set; } = true;
        public IBehaviorChain BehaviorChain { get; set; }

        public bool ShouldRun()
        {
            return ShouldRunFlag;
        }

        public virtual Task<HttpResponseMessage> Invoke()
        {
            return BehaviorChain?.InvokeNext() ?? 
                Task.FromResult(new HttpResponseMessage());
        }
    }

    public abstract class TestLoggingBehavior : TestBehavior
    {
        protected TestLoggingBehavior(Logger logger = null)
        {
            Logger = logger;
        }

        public Logger Logger { get; set; }

        public override Task<HttpResponseMessage> Invoke()
        {
            Logger.Write(GetType());
            return base.Invoke();
        }
    }

    public class TestInvokerBehavior : IBehavior
    {
        private readonly HttpResponseMessage _response;

        public TestInvokerBehavior(HttpResponseMessage response)
        {
            _response = response;
        }

        public bool ShouldRun()
        {
            return true;
        }

        public Task<HttpResponseMessage> Invoke()
        {
            return _response.ToTaskResult();
        }
    }
}
