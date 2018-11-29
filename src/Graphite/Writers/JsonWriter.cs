using System.IO;
using System.Net.Http;
using Graphite.Extensions;
using Graphite.Http;
using Newtonsoft.Json;

namespace Graphite.Writers
{
    public class JsonWriter : SerializerWriterBase
    {
        private readonly JsonSerializer _serializer;
        private readonly Configuration _configuration;

        public JsonWriter(JsonSerializer serializer,
            HttpRequestMessage requestMessage,
            HttpResponseMessage responseMessage,
            Configuration configuration) :
            base(requestMessage, responseMessage,
                configuration, MimeTypes.ApplicationJson)
        {
            _serializer = serializer;
            _configuration = configuration;
        }

        public override bool AppliesTo(ResponseWriterContext context)
        {
            return base.AppliesTo(context);
        }

        protected override void WriteToStream(ResponseWriterContext context, Stream output)
        {
            using (var streamWriter = output.CreateWriter(_configuration.DefaultEncoding,
                _configuration.SerializerBufferSize, true))
            using (var jsonWriter = new JsonTextWriter(streamWriter))
            {
                _serializer.Serialize(jsonWriter, context.Response);
                jsonWriter.Flush();
            }
        }
    }
}