using System.Collections.Generic;
using Graphite.Extensibility;

namespace Graphite.Actions
{
    public interface IActionMethodSource : IConditional
    {
        List<ActionMethod> GetActionMethods();
    }
}