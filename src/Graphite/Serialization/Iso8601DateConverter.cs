using System;
using Graphite.Extensions;
using Newtonsoft.Json;

namespace Graphite.Serialization
{
    public class Iso8601DateConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime) || objectType == typeof(DateTime?);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var datetime = value as DateTime?;
            if (datetime == null)
            {
                writer.WriteNull();
                return;
            }
            writer.WriteValue(datetime.Value.ToString("yyyy-MM-dd"));
        }

        public override object ReadJson(JsonReader reader, Type objectType,
            object existingValue, JsonSerializer serializer)
        {
            var result = reader.Value is string
                ? DateTime.Parse((string) reader.Value)
                : reader.Value;
            return (result as DateTime?)?.ToLocalDate() ?? result;
        }
    }
}