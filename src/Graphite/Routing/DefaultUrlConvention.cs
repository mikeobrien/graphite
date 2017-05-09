using System;
using System.Linq;
using Graphite.Extensions;

namespace Graphite.Routing
{
    [AttributeUsage(AttributeTargets.Method)]
    public class UrlAttribute : Attribute
    {
        public UrlAttribute(params string[] urls)
        {
            Urls = urls;
        }

        public string[] Urls { get; }
    }

    public class DefaultUrlConvention : IUrlConvention
    {
        public virtual bool AppliesTo(UrlContext context)
        {
            return true;
        }

        public virtual string[] GetUrls(UrlContext context)
        {
            var urls = context.ActionMethod.Method
                .GetAttribute<UrlAttribute>()?.Urls
                .Where(x => x.IsNotNullOrWhiteSpace())
                .Select(x => x.Trim('/')).ToArray();
            return urls ?? new[] { context.UrlSegments.Join("/") };
        }
    }
}