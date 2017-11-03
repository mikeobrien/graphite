using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Graphite.Actions;
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
            var conventionalUrl = GetUrl(context.ActionMethod, context.MethodSegments);
            var explicitUrls = context.ActionMethod.GetAttribute<UrlAttribute>()?.Urls
                .Where(x => x.IsNotNullOrWhiteSpace())
                .Select(x => x.Trim('/')).ToArray();
            var prefix = _configuration.UrlPrefix?.Trim('/');
            return (explicitUrls ?? new[] 
                {
                    prefix.IsNullOrEmpty() 
                        ? conventionalUrl 
                        : prefix.JoinUrls(conventionalUrl)
                })
                .Concat(GetUrlAliases(context))
                .ToArray();
        }
        
        protected virtual string GetUrl(ActionMethod action, List<Segment> methodSegments)
        {
            return (_configuration.HandlerNamespaceParser ??
                    DefaultHandlerNamespaceParser)(_configuration, action)
                .Split('.').Where(x => x.IsNotNullOrWhiteSpace())
                .Select(x => new Segment(x))
                .Concat(methodSegments.Select(x => x)).ToUrl();
        }

        public virtual string[] GetUrlAliases(UrlContext context)
        {
            var aliases = new List<string>();

            var attriubteAliases = context.ActionMethod.GetAttribute<UrlAliasAttribute>();
            if (attriubteAliases != null) aliases.AddRange(attriubteAliases.Urls);

            aliases.AddRange(_configuration.UrlAliases.Select(x => x(context)));

            return aliases.Where(x => x.IsNotNullOrWhiteSpace())
                .Select(x => x.Trim('/')).ToArray();
        }

        public const string HandlerNamespaceGroupName = "namespace";
        public static readonly string DefaultHandlerNamespaceConventionRegex =
            $"(?<{HandlerNamespaceGroupName}>.*)";
        public static readonly Regex DefaultHandlerNamespaceConvention =
            new Regex(DefaultHandlerNamespaceConventionRegex);

        public static string DefaultHandlerNamespaceParser(
            Configuration configuration, ActionMethod action)
        {
            return action.HandlerTypeDescriptor.Type.Namespace
                .MatchGroupValue(configuration.HandlerNamespaceConvention ??
                                 DefaultHandlerNamespaceConvention, HandlerNamespaceGroupName);
        }
    }
}