using System;

namespace Graphite.Actions
{
    public class UnhandledGraphiteRequestException : GraphiteException
    {
        public UnhandledGraphiteRequestException(Exception exception) :
            base("An unhandled Graphite request failure has occured.", exception) { }
    }
}