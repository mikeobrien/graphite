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
                    .Replace<Readers.JsonReader>().With<JsonReader>()
                    .Replace<Readers.XmlReader>().With<XmlReader>())
                .ConfigureResponseWriters(x => x
                    .Replace<Writers.JsonWriter>().With<JsonWriter>()
                    .Replace<Writers.XmlWriter>().With<XmlWriter>())
                .ConfigureRegistry(x => x.Register(options));
        }
    }
}
