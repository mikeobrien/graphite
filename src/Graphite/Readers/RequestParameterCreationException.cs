using Graphite.Actions;
using Graphite.Reflection;

namespace Graphite.Readers
{
    public class RequestParameterCreationException : GraphiteException
    {
        public RequestParameterCreationException(ParameterDescriptor parameter, 
            ActionMethod actionMethod) : base(
            $"Unable to instantiate type {parameter.ParameterType} for " +
            $"'{parameter.Name}' parameter on action {actionMethod}. " +
            "Type must have a parameterless constructor.") { }
    }
}