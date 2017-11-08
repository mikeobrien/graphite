using System.Linq;
using System.Text;
using Graphite.Extensions;

namespace Graphite.Views.ViewSource
{
    public class EmbeddedViewSource : ViewSourceBase
    {
        public EmbeddedViewSource(ViewConfiguration viewConfiguration) : 
            base(viewConfiguration) { }

        protected override ViewDescriptor[] GetViewDescriptors(ViewSourceContext context, 
            string[] viewNames, Encoding encoding)
        {
            var handlerDescriptor = context.ActionDescriptor.Action
                .HandlerTypeDescriptor;
            var @namespace = handlerDescriptor.Type.Namespace ?? "";
            var resources = handlerDescriptor.AssemblyDescriptor.Resources;

            return context.SupportedTypes
                .SelectMany(t => viewNames.Select(v => new
                {
                    Resource = resources.FirstOrDefault(x => x.Name
                        .EqualsUncase($"{@namespace}.{v}.{t}")),
                    Type = t
                }))
                .Where(x => x.Resource != null)
                .Select(x => new ViewDescriptor(x.Type,
                    () => x.Resource.GetString(encoding)))
                .ToArray();
        }
    }
}