using System.Collections.Generic;
using System.Linq;

namespace Graphite.Diagnostics
{
    public static class Extensions
    {
        public static List<IDiagnosticsSection> AsConfigured(
            this IEnumerable<IDiagnosticsSection> diagnosticsSections,
            Configuration configuration)
        {
            return configuration.DiagnosticsSections
                .PluginsFor(diagnosticsSections)
                .Select(x => x.Instance)
                .ToList();
        }
    }
}
