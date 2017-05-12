using System;
using System.Linq;
using System.Reflection;
using Graphite.Extensions;

namespace Graphite.Reflection
{
    public class ParameterDescriptor : DescriptorBase
    {
        private readonly Lazy<TypeDescriptor> _parameterType;

        public ParameterDescriptor(TypeDescriptor typeDescriptor,
            MethodDescriptor methodDescriptor,
            ParameterInfo parameterInfo, ITypeCache typeCache) :
            base(parameterInfo.Name, parameterInfo.ToLazy(
                x => x.GetCustomAttributes().ToArray()))
        {
            DeclaringType = typeDescriptor;
            Method = methodDescriptor;
            ParameterInfo = parameterInfo;
            Position = parameterInfo.Position;
            _parameterType = parameterInfo.ToLazy(
                x => typeCache.GetTypeDescriptor(x.ParameterType));
        }

        public TypeDescriptor DeclaringType { get; }
        public int Position { get; }
        public MethodDescriptor Method { get; }
        public ParameterInfo ParameterInfo { get; }
        public TypeDescriptor ParameterType => _parameterType.Value;
    }
}