using System;
using System.Linq;
using System.Reflection;
using Graphite.Extensions;

namespace Graphite.Reflection
{
    public class MethodDescriptor : DescriptorBase
    {
        private readonly Lazy<ParameterDescriptor[]> _parameters;
        private readonly Lazy<TypeDescriptor> _returnType;
        private readonly Lazy<bool> _hasResult;
        private readonly Lazy<string> _friendlyName;

        public MethodDescriptor(TypeDescriptor typeDescriptor,
            MethodInfo methodInfo, ITypeCache typeCache) :
            base(methodInfo.Name, methodInfo.ToLazy(x => CustomAttributeExtensions.GetCustomAttributes((MemberInfo) x).ToArray()))
        {
            MethodInfo = methodInfo;
            _friendlyName = methodInfo.ToLazy(x => x.GetFriendlyMethodName());
            DeclaringType = typeDescriptor;
            IsBclMethod = methodInfo.IsBclMethod();
            _parameters = methodInfo.ToLazy(x => x.GetParameters().Select(p =>
                new ParameterDescriptor(typeDescriptor, this, p, typeCache)).ToArray());
            _hasResult = methodInfo.ReturnType.ToLazy(x => x.UnwrapTask() != typeof(void));
            _returnType = methodInfo.ToLazy(x => x.ReturnType == typeof(void) ? null :
                typeCache.GetTypeDescriptor(x.ReturnType));
        }

        public TypeDescriptor DeclaringType { get; }
        public string FriendlyName => _friendlyName.Value;
        public bool IsBclMethod { get; }
        public MethodInfo MethodInfo { get; }
        public ParameterDescriptor[] Parameters => _parameters.Value;
        public bool HasResult => _hasResult.Value;
        public TypeDescriptor ReturnType => _returnType.Value;

        public override int GetHashCode()
        {
            return this.GetHashCode(MethodInfo);
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