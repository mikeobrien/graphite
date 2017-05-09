using System.Net.Http;
using System.Threading.Tasks;

namespace Graphite.Behaviors
{
    public abstract class BehaviorBase : IBehavior
    {
        protected BehaviorBase(IBehavior innerBehavior = null)
        {
            InnerBehavior = innerBehavior;
        }

        protected IBehavior InnerBehavior { get; }

        public virtual bool ShouldRun()
        {
            return true;
        }

        public abstract Task<HttpResponseMessage> Invoke();
    }
}
