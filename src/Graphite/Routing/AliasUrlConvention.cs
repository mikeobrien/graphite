using System;
using System.Collections.Generic;
using System.Linq;
using Graphite.Extensions;

namespace Graphite.Routing
{
    [AttributeUsage(AttributeTargets.Method)]
    public class UrlAliasAttribute : Attribute
    {
        public UrlAliasAttribute(params string[] urls)
        {
            Urls = urls;
        }

        public string[] Urls { get; }
    }

    public class AliasUrlConvention : IUrlConvention
    {
        private readonly Configuration _configuration;

        public AliasUrlConvention(Configuration configuration)
        {
            _configuration = configuration;
        }

        public virtual bool AppliesTo(UrlContext context)
        {
            return true;
        }

        public virtual string[] GetUrls(UrlContext context)
        {
            var aliases = new List<string>();

            var attriubteAliases = context.ActionMethod.GetAttribute<UrlAliasAttribute>();
            if (attriubteAliases != null) aliases.AddRange(attriubteAliases.Urls);

            aliases.AddRange(_configuration.UrlAliases.Select(x => 
                x(context.ActionMethod, context.Url)));

            return aliases.Where(x => x.IsNotNullOrWhiteSpace())
                .Select(x => x.Trim('/')).ToArray();
        }
    }
}