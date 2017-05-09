using System.Text;
using Bender;
using Bender.Configuration;
using Graphite.Http;
using Graphite.Writers;

namespace Graphite.Bender
{
    public class JsonWriter  : StringWriterBase
    {
        private readonly Options _options;

        public JsonWriter(Options options) : 
            base(MimeTypes.ApplicationJson, Encoding.UTF8)
        {
            _options = options;
        }

        protected override string GetResponse(ResponseWriterContext context)
        {
            return context.Response.SerializeJson(context
                .RequestContext.Route.ResponseType.Type, _options);
        }
    }
}