using System.Linq;
using Graphite.Extensions;
using Graphite.Routing;

namespace Graphite.Binding
{
    public class MapperNotFoundException : GraphiteException
    {
        public MapperNotFoundException(object[] values, ActionParameter parameter) :
            base($"Unable to map {values.Select(x => $"'{x}'").Join(", ")} " +
                 $"to type {parameter.TypeDescriptor} for '{parameter.Name}' " +
                 $"parameter on action {parameter.Action}.") { }
    }
}
