using System;
using System.Net.Http;

namespace Graphite.Actions
{
    public class GraphiteRuntimeInitializationException : Exception
    {
        public GraphiteRuntimeInitializationException(Exception exception, 
                HttpRequestMessage requestMessage, ActionMethod actionMethod) :
            base($"Graphite failed to initialize route '{requestMessage?.RequestUri}' " +
                 $"for handler '{actionMethod?.FullName}'.", exception) { }
    }
}