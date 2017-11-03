using System.Linq;
using Graphite.Extensions;

namespace Graphite.Routing
{
    public class DefaultNamespaceUrlMappingConvention : INamespaceUrlMappingConvention
    {
        private readonly Configuration _configuration;

        public DefaultNamespaceUrlMappingConvention(Configuration configuration)
        {
            _configuration = configuration;
        }

        public virtual bool AppliesTo(UrlContext context)
        {
            return true;
        }

        public virtual string[] GetUrls(UrlContext context)
        {
            var @namespace = context.ActionMethod
                .HandlerTypeDescriptor.Type.Namespace ?? "";

            return _configuration.NamespaceUrlMappings
                .Where(x => x.Namespace.IsMatch(@namespace))
                .Select(x => x.Namespace.Replace(@namespace, x.Url)
                    .Split('.', '/', '\\')
                    .Where(s => s.IsNotNullOrWhiteSpace())
                    .Select(s => new UrlSegment(s))
                    .Concat(context.MethodSegments
                        .Select(s => s)).ToUrl())
                .ToArray();
        }
    }
}