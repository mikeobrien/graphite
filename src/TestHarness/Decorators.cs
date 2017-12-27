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
            context.ActionDescriptor.Registry.Register<IDependency1, Dependency1>();
            context.ActionDescriptor.Registry.Register<IDependency2, Dependency2>();
        }
    }

    public interface IDependency1 { }
    public class Dependency1 : IDependency1 { }

    public interface IDependency2 { }
    public class Dependency2 : IDependency2 { }
}
