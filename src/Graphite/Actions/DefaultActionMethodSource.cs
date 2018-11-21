using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Graphite.Extensions;
using Graphite.Reflection;

namespace Graphite.Actions
{
    public class DefaultActionMethodSource : IActionMethodSource
    {
        public const string DefaultHandlerNameConventionRegex = "Handler$";
        public static readonly Regex DefaultHandlerNameConvention = 
            new Regex(DefaultHandlerNameConventionRegex);
        public static readonly string HttpMethodGroupName = "method";
        public static readonly string ActionSegmentsGroupName = "segments";

        private readonly Configuration _configuration;
        private readonly ITypeCache _typeCache;

        public DefaultActionMethodSource(Configuration configuration, ITypeCache typeCache)
        {
            _configuration = configuration;
            _typeCache = typeCache;
        }

        public virtual bool Applies()
        {
            return true;
        }

        public virtual List<ActionMethod> GetActionMethods()
        {
            return _configuration.Assemblies
                .SelectMany(x => _typeCache.GetAssemblyDescriptor(x).Types)
                .Where(x => 
                    (_configuration.HandlerNameConvention ?? 
                        DefaultHandlerNameConvention).IsMatch(x.Name) && 
                    (_configuration.HandlerFilter?.Invoke(_configuration, x) ?? true))
                .SelectMany(t => t.Methods
                    .Where(m => !m.MethodInfo.IsGenericMethodDefinition && !m.IsBclMethod &&
                        (_configuration.ActionNameConvention ?? DefaultActionNameConvention)
                            (_configuration).IsMatch(m.Name) && 
                        (_configuration.ActionFilter?.Invoke(_configuration, m) ?? true))
                    .Select(m => new ActionMethod(t, m)))
                .ToList();
        }

        public static Regex DefaultActionNameConvention(Configuration configuration)
        {
            return new Regex(DefaultActionNameConventionRegex(configuration));
        }

        public static string DefaultActionNameConventionRegex(Configuration configuration)
        {
            var methods = configuration.SupportedHttpMethods
                .Select(m => m.Method.InitialCap()).Join("|");
            return $"^(?<{HttpMethodGroupName}>{methods})" +
                   $"(?<{ActionSegmentsGroupName}>.*)";
        }
    }
}
