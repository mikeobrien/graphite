using System;
using Bender.Configuration;

namespace Graphite.Bender
{
    public static class Extensions
    {
        public static ConfigurationDsl UseBender(
            this ConfigurationDsl configuration, Action<OptionsDsl> configure = null)
        {
            var options = new Options();
            configure?.Invoke(new OptionsDsl(options));
            return configuration
                .ConfigureRequestReaders(x => x
                    .Replace<Readers.JsonReader>()
                    .With<JsonReader>(singleton: true))
                .ConfigureResponseWriters(x => x
                    .Replace<Writers.JsonWriter>()
                    .With<JsonWriter>(singleton: true))
                .ConfigureRegistry(x => x.Register(options));
        }
    }
}
