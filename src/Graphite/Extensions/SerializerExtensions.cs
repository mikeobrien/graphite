using System;
using System.Linq;
using System.Reflection;
using Graphite.Reflection;
using Graphite.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Graphite.Extensions
{
    public static class SerializerExtensions
    {
        public static JsonSerializerSettings SerializeIpAddresses(this JsonSerializerSettings settings)
        {
            settings.Converters.Add(new IpAddressConverter());
            return settings;
        }

        public static JsonSerializerSettings ConfigureIsoDateTimeConverter(this JsonSerializerSettings settings, 
            Action<Iso8601DateTimeConverter> configure = null)
        {
            settings.RemoveConverters<IsoDateTimeConverter>();
            configure?.Invoke(settings.GetOrAddConverter<Iso8601DateTimeConverter>());
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

        public static void AddConverter<T>(this JsonSerializerSettings settings)
            where T : JsonConverter, new()
        {
            settings.Converters.Add(new T());
        }

        public static JsonSerializerSettings RemoveConverters<T>(this JsonSerializerSettings settings) 
            where T : JsonConverter
        {
            settings.Converters.OfType<T>().ToList()
                .ForEach(x => settings.Converters.Remove(x));
            return settings;
        }

        public static JsonSerializerSettings WriteMicrosoftJsonDateTime(this JsonSerializerSettings settings,
            Action<KindDateTimeConverter> configure = null)
        {
            settings.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
            if (configure != null)
            {
                settings.RemoveConverters<KindDateTimeConverter>();
                configure(settings.GetOrAddConverter<KindDateTimeConverter>());
            }
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
