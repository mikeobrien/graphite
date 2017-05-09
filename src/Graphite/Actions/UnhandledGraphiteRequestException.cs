using System;

namespace Graphite.Actions
{
    public class UnhandledGraphiteRequestException : Exception
    {
        public UnhandledGraphiteRequestException(Exception exception) :
            base("An unhandled Graphite request failure has occured.", exception) { }
    }
}