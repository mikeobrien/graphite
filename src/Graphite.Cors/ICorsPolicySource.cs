using System.Web.Cors;
using Graphite.Extensibility;

namespace Graphite.Cors
{
    public class GraphiteCorsPolicy : CorsPolicy
    {
        public bool AllowOptionRequestsToPassThrough { get; set; } = false;
        public bool AllowRequestsWithoutOriginHeader { get; set; } = true;
        public bool AllowRequestsThatFailCors { get; set; } = true;
    }

    public interface ICorsPolicySource : IConditional
    {
        GraphiteCorsPolicy CreatePolicy();
    }
}
