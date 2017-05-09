using System.Text;
using Graphite.Extensions;
using Graphite.Http;

namespace Graphite.Writers
{
    public class JsonWriter  : StringWriterBase
    {
        public JsonWriter() : base(MimeTypes.ApplicationJson, Encoding.UTF8) { }

        protected override string GetResponse(ResponseWriterContext context)
        {
            return context.Response.SerializeJson(context
                .RequestContext.Route.ResponseType.Type);
        }
    }
}