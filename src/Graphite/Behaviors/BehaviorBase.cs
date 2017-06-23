using System.Net.Http;
using System.Threading.Tasks;

namespace Graphite.Behaviors
{
    public abstract class BehaviorBase : IBehavior
    {
        protected BehaviorBase(IBehaviorChain behaviorChain)
        {
            BehaviorChain = behaviorChain;
        }

        public IBehaviorChain BehaviorChain { get; }

        public virtual bool ShouldRun()
        {
            return true;
        }

        public abstract Task<HttpResponseMessage> Invoke();
    }
}
