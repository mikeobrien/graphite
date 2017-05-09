using Graphite.Extensions;
using Graphite.Http;

namespace Graphite.Readers
{
    public class JsonReader : StringReaderBase
    {
        public JsonReader() : base(MimeTypes.ApplicationJson) { }

        protected override object GetRequest(string data, 
            RequestReaderContext context)
        {
            return data.DeserializeJson(context.RequestContext.Route
                .RequestParameter.ParameterType.Type);
        }
    }
}