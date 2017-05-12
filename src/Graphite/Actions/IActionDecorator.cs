using System.Web.Http;
using Graphite.Extensibility;

namespace Graphite.Actions
{
    public class ActionDecoratorContext
    {
        public ActionDecoratorContext(Configuration configuration,
            HttpConfiguration httpConfiguration,
            ActionDescriptor actionDescriptor)
        {
            Configuration = configuration;
            HttpConfiguration = httpConfiguration;
            ActionDescriptor = actionDescriptor;
        }

        public virtual Configuration Configuration { get; }
        public virtual HttpConfiguration HttpConfiguration { get; }
        public virtual ActionDescriptor ActionDescriptor { get; }
    }

    public interface IActionDecorator : IConditional<ActionDecoratorContext>
    {
        void Decorate(ActionDecoratorContext context);
    }
}
