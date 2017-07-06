using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Behaviors;

namespace TestHarness.Handlers
{
    public class Behavior1 : BehaviorBase
    {
        public Behavior1(IBehaviorChain behaviorChain) : base(behaviorChain) { }

        public override Task<HttpResponseMessage> Invoke()
        {
            return BehaviorChain.InvokeNext();
        }
    }

    public class Behavior2 : BehaviorBase
    {
        public Behavior2(IBehaviorChain behaviorChain) : base(behaviorChain) { }

        public override Task<HttpResponseMessage> Invoke()
        {
            return BehaviorChain.InvokeNext();
        }
    }
}
