using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
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
        private readonly List<INamespaceUrlMappingConvention> _mappingConventions;
        private readonly HttpConfiguration _httpConfiguration;

        public DefaultUrlConvention(
            List<INamespaceUrlMappingConvention> mappingConventions,
            Configuration configuration,
            HttpConfiguration httpConfiguration)
        {
            _configuration = configuration;
            _mappingConventions = mappingConventions;
            _httpConfiguration = httpConfiguration;
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
            return _mappingConventions.ThatApplyTo(context, _configuration, _httpConfiguration)
                .SelectMany(x => x.GetUrls(context))
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