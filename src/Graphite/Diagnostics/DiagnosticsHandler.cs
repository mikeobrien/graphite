using System.IO;
using Graphite.Extensions;
using Graphite.Http;
using Graphite.Reflection;
using Graphite.Writers;

namespace Graphite.Diagnostics
{
    public class DiagnosticsHandler
    {
        private readonly IDiagnosticsProvider _diagnosticsProvider;
        private readonly ITypeCache _typeCache;
        private readonly AssemblyDescriptor _assembly;

        public DiagnosticsHandler(
            IDiagnosticsProvider diagnosticsProvider,
            ITypeCache typeCache)
        {
            _diagnosticsProvider = diagnosticsProvider;
            _typeCache = typeCache;
            _assembly = typeCache.GetTypeAssemblyDescriptor<DiagnosticsHandler>();
        }

        public string Get()
        {
            return _assembly.GetResourceString<DiagnosticsHandler>("Diagnostics.html")
                .RenderMustache(_diagnosticsProvider.GetDiagnosticsModel(), _typeCache);
        }

        [OutputStream(MimeTypes.ImagePng)]
        public Stream GetFavicon()
        {
            return _assembly.GetResourceStream<DiagnosticsHandler>("favicon.png");
        }

        [OutputStream(MimeTypes.ImagePng)]
        public Stream GetLogo()
        {
            return _assembly.GetResourceStream<DiagnosticsHandler>("logo.png");
        }
    }
}