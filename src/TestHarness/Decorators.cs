using Graphite.Actions;

namespace TestHarness.Handlers
{
    public class TestActionDecorator : IActionDecorator
    {
        public bool AppliesTo(ActionDecoratorContext context)
        {
            return context.ActionDescriptor.Route.Method == "GET";
        }

        public void Decorate(ActionDecoratorContext context)
        {
            context.ActionDescriptor.Registry.Register<IDependency, Dependency>();
        }
    }

    public interface IDependency { }
    public class Dependency : IDependency { }
}
