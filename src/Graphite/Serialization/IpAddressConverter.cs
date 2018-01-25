using System;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Graphite.Serialization
{
    public class IpAddressConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IPAddress);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((IPAddress)value).ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, 
            object existingValue, JsonSerializer serializer)
        {
            return IPAddress.Parse(JToken.Load(reader).Value<string>());
        }
    }
}
