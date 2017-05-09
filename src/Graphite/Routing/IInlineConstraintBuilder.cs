using System.Collections.Generic;

namespace Graphite.Routing
{
    public interface IInlineConstraintBuilder
    {
        List<string> Build(UrlParameter parameter);
    }
}