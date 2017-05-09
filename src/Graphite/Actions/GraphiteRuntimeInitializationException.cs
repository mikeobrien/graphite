using System;
using System.Net.Http;
using Graphite.DependencyInjection;

namespace Graphite.Actions
{
    public class GraphiteRuntimeInitializationException : Exception
    {
        public GraphiteRuntimeInitializationException(Exception exception,
            HttpRequestMessage requestMessage, ActionMethod actionMethod, IContainer container) :
            base($"Graphite failed to initialize route '{requestMessage?.RequestUri}' " +
                 $"for handler '{actionMethod?.FullName}'.", exception)
        {
            ContainerConfiguration = container.GetConfiguration();
        }

        public string ContainerConfiguration { get; }
    }
}