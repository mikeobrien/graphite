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
        private readonly Configuration _configuration;

        public DefaultUrlConvention(Configuration configuration)
        {
            _configuration = configuration;
        }

        public virtual bool AppliesTo(UrlContext context)
        {
            return true;
        }

        public virtual string[] GetUrls(UrlContext context)
        {
            var urls = context.ActionMethod.GetAttribute<UrlAttribute>()?.Urls
                .Where(x => x.IsNotNullOrWhiteSpace())
                .Select(x => x.Trim('/')).ToArray();
            var prefix = _configuration.UrlPrefix?.Trim('/');
            return urls ?? new[] { prefix.IsNullOrEmpty() 
                ? context.Url.ToString() 
                : prefix.JoinUrls(context.Url.ToString()) };
        }
    }
}