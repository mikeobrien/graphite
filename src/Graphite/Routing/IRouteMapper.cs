using Graphite.Actions;

namespace Graphite.Routing
{
    public interface IRouteMapper
    {
        void Map(ActionDescriptor actionDescriptor);
    }
}