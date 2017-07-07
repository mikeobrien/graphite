using System.IO;
using System.Linq;
using System.Text;
using Graphite.Hosting;

namespace Graphite.Views.ViewSource
{
    public class FileViewSource : ViewSourceBase
    {
        private readonly ViewConfiguration _viewConfiguration;
        private readonly IPathProvider _pathProvider;

        public FileViewSource(ViewConfiguration viewConfiguration,
            IPathProvider pathProvider) : base(viewConfiguration)
        {
            _viewConfiguration = viewConfiguration;
            _pathProvider = pathProvider;
        }

        protected override ViewDescriptor[] GetViewDescriptors(
            ViewSourceContext context, string[] viewNames, Encoding encoding)
        {
            var webRoot = _pathProvider.MapPath("~/");
            var @namespace = context.ActionDescriptor.Action
                .HandlerTypeDescriptor.Type.Namespace ?? "";

            return _viewConfiguration
                .NamespacePathMappings
                    .Where(m => m.Applies(@namespace))
                    .Select(m => m.Map(@namespace, Path.DirectorySeparatorChar))
                .SelectMany(x => context.SupportedTypes
                    .SelectMany(t => viewNames.Select(v => new
                    {
                        Path = Path.Combine(webRoot, x, $"{v}.{t}"),
                        Type = t
                    })))
                .Where(x => File.Exists(x.Path))
                .Select(x => new ViewDescriptor(x.Type,
                    () => File.ReadAllText(x.Path, encoding)))
                .ToArray();
        }
    }
}