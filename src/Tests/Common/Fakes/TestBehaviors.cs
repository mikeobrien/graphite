using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Actions;
using Graphite.Behaviors;
using Graphite.Extensions;

namespace Tests.Common.Fakes
{
    public class TestBehavior1 : TestBehavior { }
    public class TestBehavior2 : TestBehavior { }

    public class TestBehavior : IBehavior
    {
        public bool ShouldRunFlag { get; set; } = true;
        public IBehavior InnerBehavior { get; set; }

        public bool ShouldRun()
        {
            return ShouldRunFlag;
        }

        public virtual Task<HttpResponseMessage> Invoke()
        {
            return InnerBehavior.Invoke();
        }
    }

    public abstract class TestLoggingBehavior : TestBehavior
    {
        public Logger Logger { get; set; }

        public override Task<HttpResponseMessage> Invoke()
        {
            Logger.Write(GetType());
            return base.Invoke();
        }
    }

    public class TestInvokerBehavior : BehaviorBase
    {
        private readonly HttpResponseMessage _response;

        public TestInvokerBehavior(HttpResponseMessage response)
        {
            _response = response;
        }

        public override Task<HttpResponseMessage> Invoke()
        {
            return _response.ToTaskResult();
        }
    }
}
