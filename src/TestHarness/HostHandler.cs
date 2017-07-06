using Graphite.Hosting;
using Graphite.Routing;

namespace TestHarness.Host
{
    public class HostTestHandler
    {
        private readonly IPathProvider _pathProvider;

        public HostTestHandler(IPathProvider pathProvider)
        {
            _pathProvider = pathProvider;
        }

        public class PathsModel
        {
            public string ApplicationPath { get; set; }
            public string MapPath { get; set; }
        }

        public PathsModel GetPaths_Url([Wildcard] string url)
        {
            return new PathsModel
            {
                ApplicationPath = _pathProvider.ApplicationPath,
                MapPath = _pathProvider.MapPath(url)
            };
        }
    }
}