using System;

namespace Graphite
{
    public class GraphiteException : Exception
    {
        public GraphiteException(string message) : base(message) { }
        public GraphiteException(string message, Exception innerException) : 
            base(message, innerException) { }
    }
}
