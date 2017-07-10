using System;
using Graphite.Actions;

namespace Tests.Common.Fakes
{
    public class TestActionDecorator : IActionDecorator
    {
        public Func<ActionDecoratorContext, bool> AppliesToFunc { get; set; }
        public Action<ActionDecoratorContext> DecorateFunc { get; set; }
        public ActionDecoratorContext AppliesToContext { get; set; }
        public ActionDecoratorContext DecorateContext { get; set; }
        public bool AppliesToCalled { get; set; }
        public bool DecorateCalled { get; set; }

        public bool AppliesTo(ActionDecoratorContext context)
        {
            AppliesToCalled = true;
            AppliesToContext = context;
            return AppliesToFunc?.Invoke(context) ?? true;
        }

        public void Decorate(ActionDecoratorContext context)
        {
            DecorateCalled = true;
            DecorateContext = context;
            DecorateFunc?.Invoke(context);
        }
    }
}
