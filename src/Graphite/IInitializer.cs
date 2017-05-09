using System.Web.Http;

namespace Graphite
{
    public interface IInitializer
    {
        void Initialize(HttpConfiguration httpConfiguration);
    }
}