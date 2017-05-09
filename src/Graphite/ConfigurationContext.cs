using System.Web.Http;

namespace Graphite
{
    public class ConfigurationContext
    {
        public ConfigurationContext(Configuration configuration, 
            HttpConfiguration httpConfiguration)
        {
            Configuration = configuration;
            HttpConfiguration = httpConfiguration;
        }

        public Configuration Configuration { get; }
        public HttpConfiguration HttpConfiguration { get; }
    }
}
