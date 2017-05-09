using System;
using System.Linq;
using System.Reflection;
using Graphite.Extensions;

namespace Graphite.Reflection
{
    public class ParameterDescriptor : DescriptorBase
    {
        private readonly Lazy<string> _friendlyName;
        private readonly Lazy<string> _friendlyFullName;
        private readonly Lazy<TypeDescriptor> _parameterType;

        public ParameterDescriptor(TypeDescriptor typeDescriptor,
            MethodDescriptor methodDescriptor,
            ParameterInfo parameterInfo, ITypeCache typeCache) :
            base(parameterInfo.Name, parameterInfo.ToLazy(
                x => x.GetCustomAttributes().ToArray()))
        {
            _friendlyName = parameterInfo.ToLazy(x => x.GetFriendlyParameterName());
            _friendlyFullName = parameterInfo.ToLazy(x => x.GetFriendlyParameterName(true));
            DeclaringType = typeDescriptor;
            Method = methodDescriptor;
            ParameterInfo = parameterInfo;
            Position = parameterInfo.Position;
            _parameterType = parameterInfo.ToLazy(
                x => typeCache.GetTypeDescriptor(x.ParameterType));
        }

        public string FriendlyName => _friendlyName.Value;
        public string FriendlyFullName => _friendlyFullName.Value;
        public TypeDescriptor DeclaringType { get; }
        public int Position { get; }
        public MethodDescriptor Method { get; }
        public ParameterInfo ParameterInfo { get; }
        public TypeDescriptor ParameterType => _parameterType.Value;

        public override int GetHashCode()
        {
            return this.GetHashCode(ParameterInfo);
        }

        public override bool Equals(object obj)
        {
            return GetHashCode() == obj?.GetHashCode();
        }

        public override string ToString()
        {
            return FriendlyName;
        }
    }
}