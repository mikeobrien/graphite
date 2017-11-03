using System.Text.RegularExpressions;

namespace Graphite.Routing
{
    public class NamespaceUrlMapping
    {
        public NamespaceUrlMapping(Regex @namespace, string url)
        {
            Namespace = @namespace;
            Url = url;
        }

        public NamespaceUrlMapping(string @namespace, string url, bool ignoreCase = true)
            : this(ignoreCase 
                  ? new Regex(@namespace, RegexOptions.IgnoreCase) 
                  : new Regex(@namespace), url) { }

        public Regex Namespace { get; }
        public string Url { get; }

        public static readonly NamespaceUrlMapping DefaultMapping =
            new NamespaceUrlMapping(@"^[^\.]*\.?(?<namespace>.*)$", "${namespace}");

        public static NamespaceUrlMapping MapAfterNamespace(
            string @namespace, bool ignoreCase = true)
        {
            return new NamespaceUrlMapping(
                $@"^{Regex.Escape(@namespace.Trim('.'))}\.?(?<namespace>.*)$", 
                "${namespace}", ignoreCase);
        }
    }
}