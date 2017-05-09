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
            this IEnumerable<IResponseWriter> writers,
            object response, RequestContext requestContext,
            Configuration configuration)
        {
            return configuration.ResponseWriters.ThatApplyTo(writers, 
                new ResponseWriterContext(configuration, requestContext, response))
                .FirstOrDefault();
        }

        public static Task<HttpResponseMessage> Write(this IResponseWriter writer,
            Configuration configuration, RequestContext context, object response)
        {
            return writer.Write(new ResponseWriterContext(configuration, context, response));
        }
    }
}