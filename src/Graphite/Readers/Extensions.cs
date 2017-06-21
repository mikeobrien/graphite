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
        public static IRequestReader ThatApplies(
            this IEnumerable<IRequestReader> readers,
            ActionConfigurationContext actionConfigurationContext)
        {
            return actionConfigurationContext.Configuration
                .RequestReaders.ThatAppliesToOrDefault(readers,
                    actionConfigurationContext).FirstOrDefault();
        }

        public static Task<object> Read(this IRequestReader reader, 
            RequestBinderContext context)
        {
            return reader.Read();
        }
    }
}