using System.Collections;
using System.Linq;
using Graphite.DependencyInjection;
using Graphite.Extensions;
using Graphite.Reflection;

namespace Graphite.Diagnostics
{
    public class ContainersSection : DiagnosticsSectionBase
    {
        private readonly TrackingContainer _container;
        private readonly ITypeCache _typeCache;

        public ContainersSection(
            IContainer container,
            ITypeCache typeCache) : 
            base("Container")
        {
            _container = container as TrackingContainer;
            _typeCache = typeCache;
        }

        public override string Render()
        {
            return _typeCache.GetTypeAssemblyDescriptor<ContainersSection>()
                .GetResourceString<DiagnosticsHandler>("Containers.html")
                .RenderMustache(new
                {
                    registrations = GetRegistry(_container?.Registry)
                }, _typeCache);
        }

        private IEnumerable GetRegistry(Registry registry)
        {
            return registry?.OrderBy(r => r.PluginType.FullName).Select(r =>
            {
                var pluginType = _typeCache.GetTypeDescriptor(r.PluginType);
                var concreteType = r.ConcreteType != null ? _typeCache.GetTypeDescriptor(r.ConcreteType) : null;
                return new
                {
                    pluginType = pluginType.FriendlyFullName,
                    pluginAssembly = pluginType.Type.Assembly.GetFriendlyName().HtmlEncode(),
                    singleton = r.Singleton.ToYesNoModel(!r.IsInstance),
                    instance = r.IsInstance.ToYesNoModel(),
                    concreteType = concreteType?.FriendlyFullName,
                    concreteAssembly = concreteType?.Type.Assembly.GetFriendlyName().HtmlEncode()
                };
            })
            .GroupBy(x => x.pluginType)
            .Select(x => x.Select(y => y).ToListModel());
        }
    }
}