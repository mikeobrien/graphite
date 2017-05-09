using System;

namespace Graphite
{
    public class GraphiteInitializationException : Exception
    {
        public GraphiteInitializationException(Exception exception) : 
            base("Graphite failed to initialize.", exception) { }
    }
}