using Graphite.Extensibility;

namespace Graphite.Actions
{
    public class ActionDecoratorContext
    {
        public ActionDecoratorContext(ActionDescriptor actionDescriptor)
        {
            ActionDescriptor = actionDescriptor;
        }
        
        public virtual ActionDescriptor ActionDescriptor { get; }
    }

    public interface IActionDecorator : IConditional<ActionDecoratorContext>
    {
        void Decorate(ActionDecoratorContext context);
    }
}
