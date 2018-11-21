using System;
using System.Collections.Generic;
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

    [AttributeUsage(AttributeTargets.Method)]
    public class UrlAliasAttribute : Attribute
    {
        public UrlAliasAttribute(params string[] urls)
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
            var urls = context.ActionMethod
                    .GetAttribute<UrlAttribute>()?.Urls
                    .Where(x => x.IsNotNullOrWhiteSpace())
                    .Select(x => x.Trim('/')).ToArray() ??
                MapNamespaceToUrls(context);
            return urls.Concat(GetUrlAliases(context)).ToArray();
        }

        protected virtual string[] MapNamespaceToUrls(UrlContext context)
        {
            var prefix = _configuration.UrlPrefix?.Trim('/');
            var @namespace = context.ActionMethod
                .HandlerTypeDescriptor.Type.Namespace ?? "";

            return _configuration.NamespaceUrlMappings
                .Where(x => x.Applies(@namespace))
                .Select(x => x.Map(@namespace, s => new UrlSegment(s))
                    .Concat(context.MethodSegments
                        .Select(s => s))
                    .ToUrl())
                .Select(x => prefix.IsNotNullOrEmpty() ? prefix.JoinUrls(x) : x)
                .ToArray();
        }

        public virtual string[] GetUrlAliases(UrlContext context)
        {
            var aliases = new List<string>();

            var attributeAliases = context.ActionMethod.GetAttribute<UrlAliasAttribute>();
            if (attributeAliases != null) aliases.AddRange(attributeAliases.Urls);

            aliases.AddRange(_configuration.UrlAliases.Select(x => x(context)));

            return aliases.Where(x => x.IsNotNullOrWhiteSpace())
                .Select(x => x.Trim('/')).ToArray();
        }
    }
}