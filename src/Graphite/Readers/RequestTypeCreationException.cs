using Graphite.Actions;
using Graphite.Reflection;

namespace Graphite.Readers
{
    public class RequestTypeCreationException : GraphiteException
    {
        public RequestTypeCreationException(TypeDescriptor type, 
            ActionMethod actionMethod) : base(
            $"Unable to instantiate type {type} for action {actionMethod}. " +
                "Type must have a parameterless constructor.") { }
    }
}