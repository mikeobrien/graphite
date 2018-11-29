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
            settings.TryAddConverter(configure);
            return settings;
        }

        public static JsonSerializerSettings WriteMicrosoftJsonDateTime(this JsonSerializerSettings settings,
            Action<KindDateTimeConverter> configure = null)
        {
            settings.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
            settings.RemoveConverters<KindDateTimeConverter>();
            settings.TryAddConverter(configure);
            return settings;
        }

        public static JsonSerializerSettings TryAddConverter<T>(this JsonSerializerSettings settings, Action<T> configure = null)
            where T : JsonConverter, new()
        {
            var converter = settings.Converters.OfType<T>().FirstOrDefault();
            if (converter == null)
            {
                converter = new T();
                settings.Converters.Add(converter);
            }
            configure?.Invoke(converter);
            return settings;
        }

        public static JsonSerializerSettings AddConverter<T>(this JsonSerializerSettings settings, Action<T> configure = null)
            where T : JsonConverter, new()
        {
            var converter = new T();
            configure?.Invoke(converter);
            settings.Converters.Add(converter);
            return settings;
        }

        public static JsonSerializerSettings RemoveConverters<T>(this JsonSerializerSettings settings) 
            where T : JsonConverter
        {
            settings.Converters.OfType<T>().ToList()
                .ForEach(x => settings.Converters.Remove(x));
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

        public static JsonSerializerSettings PrettyPrintInDebugMode<T>(this JsonSerializerSettings settings)
        {
            if (typeof(T).Assembly.IsInDebugMode())
                settings.Formatting = Formatting.Indented;
            return settings;
        }
    }
}
