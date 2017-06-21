using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Actions;
using Graphite.Extensibility;

namespace Graphite.Writers
{
    public static class Extensions
    {
        public static IResponseWriter ThatAppliesTo(
            this IEnumerable<IResponseWriter> writers, object response,
            ActionConfigurationContext actionConfigurationContext)
        {
            return writers.ThatApply(response, actionConfigurationContext)
                .FirstOrDefault();
        }

        public static List<IResponseWriter> ThatApply(
            this IEnumerable<IResponseWriter> writers, object response,
            ActionConfigurationContext actionConfigurationContext)
        {
            return actionConfigurationContext.Configuration.ResponseWriters
                .ThatAppliesToOrDefault(writers, actionConfigurationContext,
                    new ResponseWriterContext(response))
                .OrderBy(x => x.IsWeighted)
                .ThenByDescending(x => x.Weight)
                .ThenBy(x => actionConfigurationContext
                    .Configuration.ResponseWriters.Order(x))
                .ToList();
        }

        public static Task<HttpResponseMessage> Write(this IResponseWriter writer, object response)
        {
            return writer.Write(new ResponseWriterContext(response));
        }
    }
}