using Graphite.Http;
using Newtonsoft.Json;

namespace Graphite.Readers
{
    public class JsonReader : StringReaderBase
    {
        private readonly JsonSerializer _serializer;

        public JsonReader(JsonSerializer serializer) : 
            base(MimeTypes.ApplicationJson)
        {
            _serializer = serializer;
        }

        protected override ReadResult GetRequest(string data, ReaderContext context)
        {
            try
            {
                var jsonReader = new JsonTextReader(new System.IO.StringReader(data));
                return ReadResult.Success(_serializer.Deserialize(
                    jsonReader, context.ReadType.Type));
            }
            catch (JsonReaderException exception)
            {
                return ReadResult.Failure(exception.Message);
            }
            catch (JsonSerializationException exception)
            {
                return ReadResult.Failure(exception.Message);
            }
        }
    }
}