using System;
using System.Linq;
using System.Reflection;

namespace Graphite.Setup
{
    public partial class ConfigurationDsl
    {
        /// <summary>
        /// Includes the assembly of the specified type.
        /// This call is additive, so you can specify multiple assemblies.
        /// </summary>
        public ConfigurationDsl IncludeTypeAssembly<T>()
        {
            IncludeTypeAssemblies(typeof(T));
            return this;
        }

        /// <summary>
        /// Includes the assembly of the specified type.
        /// This call is additive, so you can specify multiple assemblies..
        /// </summary>
        public ConfigurationDsl IncludeTypeAssemblies(params Type[] types)
        {
            return IncludeAssemblies(types.Select(x => x.Assembly).ToArray());
        }

        /// <summary>
        /// Includes the specified assemblies.
        /// </summary>
        public ConfigurationDsl IncludeAssemblies(params Assembly[] assemblies)
        {
            _configuration.Assemblies.AddRange(assemblies
                .Where(x => !_configuration.Assemblies.Contains(x)));
            return this;
        }
    }
}
