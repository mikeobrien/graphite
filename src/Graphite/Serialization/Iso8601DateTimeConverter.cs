using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Graphite.Serialization
{
    public class Iso8601DateTimeConverter : IsoDateTimeConverter
    {
        private bool _adjustToLocal;

        /// <summary>
        /// Converts to local time after deserialization.
        /// </summary>
        public Iso8601DateTimeConverter AdjustToLocalAfterDeserializing()
        {
            _adjustToLocal = true;
            return this;
        }

        public override object ReadJson(JsonReader reader, Type objectType, 
            object existingValue, JsonSerializer serializer)
        {
            
            var result = base.ReadJson(reader, objectType, existingValue, serializer);
            if (_adjustToLocal && result is DateTime)
                return ((DateTime) result).ToLocalTime();
            return result;
        }
    }
}