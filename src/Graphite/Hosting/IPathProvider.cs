namespace Graphite.Hosting
{
    public interface IPathProvider
    {
        string ApplicationPath { get; }
        string MapPath(string virtualPath);
    }
}
