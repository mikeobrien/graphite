using Graphite.Extensions;

namespace Graphite.Http
{
    public class RemoteHost
    {
        public const string RemoteHostProperty = "RemoteHost";
        public const string RemotePortProperty = "RemotePort";

        public RemoteHost(IRequestPropertiesProvider requestPropertiesProvider)
        {
            var properties = requestPropertiesProvider.GetProperties();

            Host = properties[RemoteHostProperty].ToString();
            Port = properties[RemotePortProperty].TryParseInt();
        }

        public string Host { get; }
        public int Port { get; }
    }
}
