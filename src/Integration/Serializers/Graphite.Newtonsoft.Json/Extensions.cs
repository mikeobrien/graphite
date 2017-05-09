using System;
using Newtonsoft.Json;

namespace Graphite.Newtonsoft.Json
{
    public static class Extensions
    {
        public static ConfigurationDsl UseJsonNet(
            this ConfigurationDsl configuration, 
            Action<JsonSerializerSettings> configure = null)
        {
            var settings = new JsonSerializerSettings();
            configure?.Invoke(settings);
            return configuration
                .ConfigureRequestReaders(x => x
                    .Replace<Readers.JsonReader>()
                    .With<JsonReader>(singleton: true))
                .ConfigureResponseWriters(x => x
                    .Replace<Writers.JsonWriter>()
                    .With<JsonWriter>(singleton: true))
                .ConfigureRegistry(x => x.Register(settings));
        }
    }
}
