using System;

namespace Graphite.Actions
{
    public class GraphiteRuntimeInitializationException : Exception
    {
        public GraphiteRuntimeInitializationException(Exception exception, RequestContext requestContext) :
            base($"Graphite failed to initialize route '{requestContext.RequestMessage?.RequestUri}' " +
                 $"for handler '{requestContext.Action?.FullName}'.", exception) { }
    }
}