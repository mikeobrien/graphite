using Graphite.Actions;
using Graphite.Extensibility;

namespace Graphite.Views.ViewSource
{
    public class ViewSourceContext
    {
        public ViewSourceContext(ActionDescriptor actionDescriptor, 
            string[] supportedTypes)
        {
            ActionDescriptor = actionDescriptor;
            SupportedTypes = supportedTypes;
        }
        
        public ActionDescriptor ActionDescriptor { get; }
        public string[] SupportedTypes { get; }
    }

    public interface IViewSource : IConditional<ViewSourceContext>
    {
        View[] GetViews(ViewSourceContext context);
    }
}
