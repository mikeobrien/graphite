using System;
using System.IO;
using Graphite.Extensions;
using Graphite.Hosting;

namespace Graphite.Owin
{
    public class OwinPathProvider : IPathProvider
    {
        private static readonly string ApplicationBase = 
            AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
        
        public string ApplicationPath => ApplicationBase;

        public string MapPath(string virtualPath)
        {
            return virtualPath.IsNullOrEmpty() ? ApplicationBase : 
                Path.GetFullPath(Path.Combine(ApplicationBase, 
                    virtualPath.Trim('~', '/', '\\').Replace("/", @"\")));
        }
    }
}