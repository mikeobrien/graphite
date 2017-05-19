using System;
using System.Linq;
using System.Reflection;
using Graphite.Extensions;

namespace Graphite.Reflection
{
    public class TypeDescriptor : DescriptorBase
    {
        private readonly Lazy<bool> _isSimpleType;
        private readonly Lazy<string> _friendlyName;
        private readonly Lazy<string> _friendlyFullName;
        private readonly Lazy<MethodDescriptor[]> _methods;
        private readonly Lazy<PropertyDescriptor[]> _properties;
        private readonly Lazy<TypeDescriptor> _arrayElementType;
        private readonly Lazy<TypeDescriptor> _genericListCastableType;
        private readonly Lazy<bool> _isNullable;
        private readonly Lazy<TypeDescriptor> _underlyingNullableType;
        private readonly Lazy<Func<object>> _tryCreate;

        public TypeDescriptor(Type type, ITypeCache typeCache) : 
            this(type.UnwrapTask(), type, typeCache) { }

        private TypeDescriptor(Type type, Type originalType, ITypeCache typeCache)
           : base(type.Name, type.ToLazy(x => x.GetCustomAttributes().ToArray()))
        {
            Type = type;
            IsBclType = type.IsBclType();
            _isSimpleType = type.ToLazy(x => x.IsSimpleType());
            _friendlyName = type.ToLazy(x => x.GetFriendlyTypeName());
            _friendlyFullName = type.ToLazy(x => x.GetFriendlyTypeName(true));
            _tryCreate = type.ToLazy(x => x.CompileTryCreate());

            _methods = type.ToLazy(x => x.GetMethods().Select(y => 
                new MethodDescriptor(this, y, typeCache)).ToArray());
            _properties = type.ToLazy(x => x.GetProperties().Select(y => 
                new PropertyDescriptor(this, y, typeCache)).ToArray());

            _isNullable = type.ToLazy(x => x.IsNullable());
            _underlyingNullableType = type.ToLazy(x => typeCache
                .GetTypeDescriptor(x.GetUnderlyingNullableType()));

            _arrayElementType = type.ToLazy(x => typeCache
                .GetTypeDescriptor(x.TryGetArrayElementType()));
            _genericListCastableType = type.ToLazy(x => typeCache
                .GetTypeDescriptor(x.TryGetGenericListCastableElementType()));

            IsTask = originalType.IsTask();
            IsTaskWithResult = !IsTask && type != originalType;
        }

        public Type Type { get; }
        public string FriendlyName => _friendlyName.Value;
        public string FriendlyFullName => _friendlyFullName.Value;
        public bool IsBclType { get; }
        public bool IsSimpleType => _isSimpleType.Value;
        public bool HasSimpleElementType => ElementType?.IsSimpleType ?? false;
        public bool IsSimpleTypeOrHasSimpleElementType => IsSimpleType || HasSimpleElementType;
        public bool IsComplexType => !_isSimpleType.Value;
        public bool HasComplexElementType => ElementType?.IsComplexType ?? false;
        public bool IsComplexTypeOrHasComplexElementType => (!HasElement && IsComplexType) || 
            (HasElement && HasComplexElementType);
        public MethodDescriptor[] Methods => _methods.Value;
        public PropertyDescriptor[] Properties => _properties.Value;
        public bool IsNullable => _isNullable.Value;
        public TypeDescriptor UnderlyingNullableType => _underlyingNullableType.Value;
        public bool IsArray => ArrayElementType != null;
        public TypeDescriptor ArrayElementType => _arrayElementType.Value;
        public bool IsGenericListCastable => GenericListCastableElementType != null;
        public TypeDescriptor GenericListCastableElementType => _genericListCastableType.Value;
        public bool HasElement => IsArray || IsGenericListCastable;
        public TypeDescriptor ElementType => ArrayElementType ?? GenericListCastableElementType;
        public bool IsTask { get; }
        public bool IsTaskWithResult { get; }

        public object TryCreate()
        {
            return _tryCreate.Value();
        }

        public override int GetHashCode()
        {
            return this.GetHashCode(Type);
        }

        public override bool Equals(object obj)
        {
            return GetHashCode() == obj?.GetHashCode();
        }

        public override string ToString()
        {
            return FriendlyFullName;
        }
    }
}