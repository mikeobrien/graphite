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
        public static List<IResponseWriter> ThatApply(
            this IEnumerable<IResponseWriter> writers, object response,
            ActionDescriptor actionDescriptor)
        {
            var writerPlugins = actionDescriptor.ResponseWriters;
            return writerPlugins
                .ThatApplyToOrDefault(writers, new ResponseWriterContext(response))
                .OrderBy(x => x.IsWeighted)
                .ThenByDescending(x => x.Weight)
                .ThenBy(x => writerPlugins.IndexOf(x))
                .ToList();
        }

        public static Task<HttpResponseMessage> Write(this IResponseWriter writer, object response)
        {
            return writer.Write(new ResponseWriterContext(response));
        }
    }
}