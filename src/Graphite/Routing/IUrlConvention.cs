using Graphite.Extensibility;

namespace Graphite.Routing
{
    public interface IUrlConvention : IConditional<UrlContext>
    {
        string[] GetUrls(UrlContext context);
    }
}
