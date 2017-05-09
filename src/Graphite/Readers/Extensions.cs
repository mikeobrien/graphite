using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graphite.Actions;
using Graphite.Binding;
using Graphite.Extensibility;

namespace Graphite.Readers
{
    public static class Extensions
    {
        public static IRequestReader ThatApplyTo(
            this IEnumerable<IRequestReader> readers, RequestContext requestContext, 
            Configuration configuration)
        {
            return configuration.RequestReaders.ThatApplyTo(readers, 
                new RequestReaderContext(configuration, requestContext)).FirstOrDefault();
        }

        public static Task<object> Read(this IRequestReader reader, RequestBinderContext context)
        {
            return reader.Read(new RequestReaderContext(context.Configuration, context.RequestContext));
        }
    }
}