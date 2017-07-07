using System;
using System.Linq;
using System.Reflection;
using Graphite.Extensions;

namespace Graphite.Reflection
{
    public class AssemblyDescriptor
    {
        private readonly Lazy<TypeDescriptor[]> _types;
        private readonly Lazy<AssemblyResource[]> _resources;

        public AssemblyDescriptor(Assembly assembly, Func<Assembly, TypeDescriptor[]> types)
        {
            Assembly = assembly;
            _types = assembly.ToLazy(types);
            _resources = assembly.ToLazy(x => x.GetManifestResourceNames()
                .Select(n => new AssemblyResource(n, assembly)).ToArray());
        }

        public Assembly Assembly { get; }
        public TypeDescriptor[] Types => _types.Value;
        public AssemblyResource[] Resources => _resources.Value;
    }
}