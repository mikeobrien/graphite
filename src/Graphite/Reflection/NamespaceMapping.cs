using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Graphite.Extensions;

namespace Graphite.Reflection
{
    public class NamespaceMapping
    {
        public static readonly char[] KnowDelimiters = { '.', '/', '\\' };
        private readonly Regex _namespace;
        private readonly string _result;

        public NamespaceMapping(Regex @namespace, string result)
        {
            _namespace = @namespace;
            _result = result;
        }

        public NamespaceMapping(string @namespace, string result, bool ignoreCase = true)
            : this(ignoreCase 
                  ? new Regex(@namespace, RegexOptions.IgnoreCase) 
                  : new Regex(@namespace), result) { }

        public bool Applies(string @namespace)
        {
            return _namespace.IsMatch(@namespace);
        }

        public string Map(string @namespace, char delimiter)
        {
            return Map(@namespace).Join(delimiter);
        }

        public IEnumerable<T> Map<T>(string @namespace, Func<string, T> map)
        {
            return Map(@namespace).Select(map);
        }

        private IEnumerable<string> Map(string @namespace)
        {
            return _namespace
                .Replace(@namespace, _result)
                .Split(KnowDelimiters)
                .Where(s => s.IsNotNullOrWhiteSpace());
        }

        public static readonly NamespaceMapping DefaultMapping =
            new NamespaceMapping(@"^[^\.]*\.?(?<namespace>.*)$", "${namespace}");

        public static NamespaceMapping MapAfterNamespace(
            string @namespace, bool ignoreCase = true)
        {
            return new NamespaceMapping(
                $@"^{Regex.Escape(@namespace.Trim('.'))}\.?(?<namespace>.*)$", 
                "${namespace}", ignoreCase);
        }
    }
}