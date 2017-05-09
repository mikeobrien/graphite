using System;
using System.Reflection;

namespace Graphite.Reflection
{
    public interface ITypeCache
    {
        TypeDescriptor[] GetTypeDescriptors(Assembly assembly);
        TypeDescriptor GetTypeDescriptor(Type type);
    }
}