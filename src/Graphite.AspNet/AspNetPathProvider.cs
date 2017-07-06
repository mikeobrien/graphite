using System.Web.Hosting;
using Graphite.Hosting;

namespace Graphite.AspNet
{
    public class AspNetPathProvider : IPathProvider
    {
        public string ApplicationPath => HostingEnvironment.ApplicationPhysicalPath;

        public string MapPath(string virtualPath)
        {
            return HostingEnvironment.MapPath(virtualPath);
        }
    }
}