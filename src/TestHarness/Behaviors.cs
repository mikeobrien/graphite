using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Actions;

namespace TestHarness
{
    public class EmptyBehavior : BehaviorBase
    {
        public EmptyBehavior(IBehavior innerBehavior) : base(innerBehavior) { }
        public override Task<HttpResponseMessage> Invoke()
        {
            return InnerBehavior.Invoke();
        }
    }

    public class Behavior1 : EmptyBehavior
    {
        public Behavior1(IBehavior innerBehavior) : base(innerBehavior) { }
    }

    public class Behavior2 : EmptyBehavior
    {
        public Behavior2(IBehavior innerBehavior) : base(innerBehavior) { }
    }

    public class Behavior3 : EmptyBehavior
    {
        public Behavior3(IBehavior innerBehavior) : base(innerBehavior) { }
    }

    public class Behavior4 : EmptyBehavior
    {
        public Behavior4(IBehavior innerBehavior) : base(innerBehavior) { }
    }

    public class Behavior5 : EmptyBehavior
    {
        public Behavior5(IBehavior innerBehavior) : base(innerBehavior) { }
    }

    public class Behavior6 : EmptyBehavior
    {
        public Behavior6(IBehavior innerBehavior) : base(innerBehavior) { }
    }

    public class Behavior7 : EmptyBehavior
    {
        public Behavior7(IBehavior innerBehavior) : base(innerBehavior) { }
    }

    public class Behavior8 : EmptyBehavior
    {
        public Behavior8(IBehavior innerBehavior) : base(innerBehavior) { }
    }

    public class Behavior9 : EmptyBehavior
    {
        public Behavior9(IBehavior innerBehavior) : base(innerBehavior) { }
    }

    public class Behavior10 : EmptyBehavior
    {
        public Behavior10(IBehavior innerBehavior) : base(innerBehavior) { }
    }
}