using Graphite.Extensibility;

namespace Graphite.Routing
{
    public interface INamespaceUrlMappingConvention : IConditional<UrlContext>
    {
        string[] GetUrls(UrlContext context);
    }
}