using System;
using Newtonsoft.Json;

namespace Graphite.Serialization
{
    public class KindDateTimeConverter : JsonConverter
    {
        private bool _adjustToLocal;
        private bool _adjustToUtc;

        /// <summary>
        /// Converts to local time after deserialization.
        /// </summary>
        public KindDateTimeConverter AdjustToLocalAfterDeserializing()
        {
            _adjustToLocal = true;
            return this;
        }

        /// <summary>
        /// Converts to utc time before deserialization.
        /// </summary>
        public KindDateTimeConverter AdjustToUtcBeforeSerializing()
        {
            _adjustToUtc = true;
            return this;
        }

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
            writer.WriteValue(_adjustToUtc
                ? datetime.Value.ToUniversalTime()
                : datetime.Value);
        }

        public override object ReadJson(JsonReader reader, Type objectType,
            object existingValue, JsonSerializer serializer)
        {
            var datetime = reader.Value is string
                ? DateTime.Parse((string) reader.Value)
                : reader.Value;
            return datetime is DateTime && _adjustToLocal
                ? ((DateTime)datetime).ToLocalTime()
                : datetime;
        }
    }
}