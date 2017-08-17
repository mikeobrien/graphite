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
            IncludeTypeAssembly(typeof(T));
            return this;
        }

        /// <summary>
        /// Includes the assembly of the specified type.
        /// This call is additive, so you can specify multiple assemblies..
        /// </summary>
        public ConfigurationDsl IncludeTypeAssembly(Type type)
        {
            return IncludeAssemblies(type.Assembly);
        }

        /// <summary>
        /// Includes the current assemby.
        /// </summary>
        public ConfigurationDsl IncludeThisAssembly()
        {
            IncludeAssemblies(Assembly.GetCallingAssembly());
            return this;
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

        /// <summary>
        /// Clears the default assemblies.
        /// </summary>
        public ConfigurationDsl ClearAssemblies()
        {
            _configuration.Assemblies.Clear();
            return this;
        }
    }
}
