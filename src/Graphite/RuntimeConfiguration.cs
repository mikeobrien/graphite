using System.Collections.Generic;
using Graphite.Actions;

namespace Graphite
{
    public class RuntimeConfiguration
    {
        public RuntimeConfiguration(IEnumerable<ActionDescriptor> actionDescriptors)
        {
            Actions = actionDescriptors;
        }

        public IEnumerable<ActionDescriptor> Actions { get; }
    }
}
