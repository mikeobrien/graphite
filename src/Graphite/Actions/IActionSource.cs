using System.Collections.Generic;
using Graphite.Extensibility;

namespace Graphite.Actions
{
    public interface IActionSource : IConditional
    {
        List<ActionDescriptor> GetActions();
    }
}
