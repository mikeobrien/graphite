using System;
using System.Linq;
using System.Reflection;
using Graphite.Extensions;

namespace Graphite.Reflection
{
    public class TypeCache : ITypeCache
    {
        private static readonly Func<Type, TypeDescriptor> TypeDescriptors =
            Memoize.Func<Type, TypeDescriptor>(x => new TypeDescriptor(x, Instance));

        private static readonly Func<Assembly, TypeDescriptor[]> AssemblyTypes = 
            Memoize.Func<Assembly, TypeDescriptor[]>(x => x
                .GetTypesSafely().Select(TypeDescriptors).ToArray());

        private static ITypeCache Instance => new TypeCache();

        public TypeDescriptor[] GetTypeDescriptors(Assembly assembly)
        {
            return assembly == null ? null : AssemblyTypes(assembly);
        }
        
        public TypeDescriptor GetTypeDescriptor(Type type)
        {
            return type == null ? null : TypeDescriptors(type);
        }
    }
}
