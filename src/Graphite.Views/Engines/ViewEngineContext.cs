using Graphite.Actions;

namespace Graphite.Views.Engines
{
    public class ViewEngineContext
    {
        public ViewEngineContext(ActionDescriptor actionDescriptor)
        {
            ActionDescriptor = actionDescriptor;
        }

        public ActionDescriptor ActionDescriptor { get; }
    }
}
