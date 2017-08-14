using System;
using System.Linq;
using System.Net;
using System.Reflection;
using Graphite.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Graphite.Extensions
{
    public static class SerializerExtensions
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

        public static JsonSerializerSettings SerializeIpAddresses(this JsonSerializerSettings settings)
        {
            settings.Converters.Add(new IpAddressConverter());
            return settings;
        }

        public class GraphiteIsoDateTimeConverter : IsoDateTimeConverter
        {
            /// <summary>
            /// Converts to local time after deserialization.
            /// </summary>
            public bool AdjustToLocalAfterDeserializing { get; set; }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var result = base.ReadJson(reader, objectType, existingValue, serializer);
                if (AdjustToLocalAfterDeserializing && result is DateTime)
                    return ((DateTime) result).ToLocalTime();
                return result;
            }
        }

        public static JsonSerializerSettings ConfigureIsoDateTimeConverter(this JsonSerializerSettings settings, 
            Action<GraphiteIsoDateTimeConverter> configure)
        {
            settings.RemoveConverters<IsoDateTimeConverter>();
            configure(settings.GetOrAddConverter<GraphiteIsoDateTimeConverter>());
            return settings;
        }

        public static T GetOrAddConverter<T>(this JsonSerializerSettings settings)
            where T : JsonConverter, new()
        {
            var converter = settings.Converters.OfType<T>().FirstOrDefault();
            if (converter != null) return converter;
            converter = new T();
            settings.Converters.Add(converter);
            return converter;
        }

        public static JsonSerializerSettings RemoveConverters<T>(this JsonSerializerSettings settings) 
            where T : JsonConverter
        {
            settings.Converters.OfType<T>().ToList()
                .ForEach(x => settings.Converters.Remove(x));
            return settings;
        }

        public static JsonSerializerSettings WriteMicrosoftJsonDateTime(this JsonSerializerSettings settings)
        {
            settings.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
            return settings;
        }

        public static JsonSerializerSettings WriteEnumNames(this JsonSerializerSettings settings)
        {
            settings.Converters.Add(new StringEnumConverter());
            return settings;
        }

        public static JsonSerializerSettings WriteNonNumericFloatsAsDefault(this JsonSerializerSettings settings)
        {
            settings.FloatFormatHandling = FloatFormatHandling.DefaultValue;
            return settings;
        }

        public static JsonSerializerSettings UseCamelCaseNaming(this JsonSerializerSettings settings)
        {
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            return settings;
        }

        public static JsonSerializerSettings IgnoreCircularReferences(this JsonSerializerSettings settings)
        {
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return settings;
        }

        public static JsonSerializerSettings IgnoreNullValues(this JsonSerializerSettings settings)
        {
            settings.NullValueHandling = NullValueHandling.Ignore;
            return settings;
        }

        public static JsonSerializerSettings FailOnUnmatchedElements(this JsonSerializerSettings settings)
        {
            settings.MissingMemberHandling = MissingMemberHandling.Error;
            return settings;
        }

        public static JsonSerializerSettings PrettyPrintInDebugMode(this JsonSerializerSettings settings)
        {
            if (Assembly.GetCallingAssembly().IsInDebugMode())
                settings.Formatting = Formatting.Indented;
            return settings;
        }
    }
}
