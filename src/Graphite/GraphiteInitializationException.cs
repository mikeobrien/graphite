using System;

namespace Graphite
{
    public class GraphiteInitializationException : GraphiteException
    {
        public GraphiteInitializationException(Exception exception) : 
            base("Graphite failed to initialize.", exception) { }
    }
}