using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Http;
using Newtonsoft.Json;

namespace Graphite.Writers
{
    public class JsonWriter : PushWriterBase
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
        
        protected override Task WriteResponse(ResponseWriterContext context,
            Stream stream, TransportContext transportContext)
        {
            var jsonWriter = new JsonTextWriter(new System.IO
                .StreamWriter(stream, _configuration.DefaultEncoding));
            _serializer.Serialize(jsonWriter, context.Response);
            jsonWriter.Flush();
            return Task.CompletedTask;
        }
    }
}