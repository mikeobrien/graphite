using System;
using Graphite.Actions;
using Graphite.DependencyInjection;

namespace Graphite.Exceptions
{
    public class UnhandledGraphiteException : Exception
    {
        public UnhandledGraphiteException(ActionDescriptor actionDescriptor,
            IContainer container, Exception exception) : 
            base("An unhandled Graphite exception has occured.", exception)
        {
            ActionDescriptor = actionDescriptor;
            Container = container;
        }

        public ActionDescriptor ActionDescriptor { get; set; }
        public IContainer Container { get; set; }
    }
}