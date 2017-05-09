using System.Collections.Generic;
using System.Web.Http;
using Graphite.Extensibility;

namespace Graphite.Actions
{
    public class ActionSourceContext
    {
        public ActionSourceContext(Configuration configuration, 
            HttpConfiguration httpConfiguration)
        {
            Configuration = configuration;
            HttpConfiguration = httpConfiguration;
        }

        public virtual Configuration Configuration { get; }
        public virtual HttpConfiguration HttpConfiguration { get; }
    }

    public interface IActionSource : IConditional<ActionSourceContext>
    {
        List<ActionDescriptor> GetActions(ActionSourceContext context);
    }
}
