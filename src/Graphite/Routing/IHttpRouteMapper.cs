using Graphite.Actions;

namespace Graphite.Routing
{
    public interface IHttpRouteMapper
    {
        void Map(ActionDescriptor actionDescriptor);
    }
}