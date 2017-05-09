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
        public static IResponseWriter ThatApplyTo(
            this IEnumerable<IResponseWriter> writers, object response,
            ActionConfigurationContext actionConfigurationContext)
        {
            return actionConfigurationContext.Configuration.ResponseWriters.ThatApplyTo(writers,
                actionConfigurationContext, new ResponseWriterContext(response))
                .FirstOrDefault();
        }

        public static Task<HttpResponseMessage> Write(this IResponseWriter writer, object response)
        {
            return writer.Write(new ResponseWriterContext(response));
        }
    }
}