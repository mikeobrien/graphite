using System.Collections.Generic;

namespace Graphite.Http
{
    public interface IRequestPropertiesProvider
    {
        IDictionary<string, object> GetProperties();
    }
}
