using System.Collections.Generic;
using System.Web.Http;
using Graphite.Extensibility;

namespace Graphite.Actions
{
    public class ActionMethodSourceContext
    {
        public ActionMethodSourceContext(Configuration configuration, 
            HttpConfiguration httpConfiguration)
        {
            Configuration = configuration;
            HttpConfiguration = httpConfiguration;
        }

        public virtual Configuration Configuration { get; }
        public virtual HttpConfiguration HttpConfiguration { get; }
    }

    public interface IActionMethodSource : IConditional<ActionMethodSourceContext>
    {
        IEnumerable<ActionMethod> GetActionMethods(ActionMethodSourceContext context);
    }
}