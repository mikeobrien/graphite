using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Graphite.Reflection;

namespace Graphite.Diagnostics
{
    public interface IDiagnosticsProvider
    {
        object GetDiagnosticsModel();
    }
    
    public class DiagnosticsProvider : IDiagnosticsProvider
    {
        private readonly Configuration _configuration;
        private readonly List<IDiagnosticsSection> _sections;
        private readonly AssemblyDescriptor _assembly;

        public DiagnosticsProvider(
            Configuration configuration, 
            ITypeCache typeCache,
            List<IDiagnosticsSection> sections)
        {
            _configuration = configuration;
            _sections = sections;
            _assembly = typeCache.GetTypeAssemblyDescriptor<DiagnosticsProvider>();
        }

        public object GetDiagnosticsModel()
        {
            var sections = _sections.AsConfigured(_configuration);
            return new
            {
                url = $"/{_configuration.DiagnosticsUrl.Trim('/')}",
                styles = _assembly.GetResourceString<DiagnosticsHandler>("Diagnostics.css"),
                scripts = _assembly.GetResourceString<DiagnosticsHandler>("Diagnostics.js"),
                version = Assembly.GetExecutingAssembly().GetName().Version,
                tabs = sections.Select((x, i) => new 
                    {
                        id = x.Id,
                        name = x.Name,
                        selected = i == 0
                    }),
                sectionCount = sections.Count + 1,
                sections = sections.Select((x, i) => new 
                    {
                        id = x.Id,
                        template = x.Render(),
                        hidden = i > 0
                    })
            };
        }
    }
}
