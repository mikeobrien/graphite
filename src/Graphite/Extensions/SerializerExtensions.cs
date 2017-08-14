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

        public static JsonSerializerSettings ConfigureIsoDateTimeConverter(this JsonSerializerSettings settings, 
            Action<IsoDateTimeConverter> configure)
        {
            var converter = settings.Converters.OfType<IsoDateTimeConverter>().FirstOrDefault();
            if (converter == null)
            {
                converter = new IsoDateTimeConverter();
                settings.Converters.Add(converter);
            }
            configure(converter);
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
