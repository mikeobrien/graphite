using System;
using System.Reflection;

namespace Graphite.Reflection
{
    public interface ITypeCache
    {
        AssemblyDescriptor GetAssemblyDescriptor(Assembly assembly);
        TypeDescriptor GetTypeDescriptor(Type type);
    }
}