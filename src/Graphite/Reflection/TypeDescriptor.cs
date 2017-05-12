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
    }

    public class MethodDescriptor : DescriptorBase
    {
        private readonly Lazy<ParameterDescriptor[]> _parameters;
        private readonly Lazy<TypeDescriptor> _returnType;
        private readonly Lazy<bool> _hasResult;
        private readonly Lazy<string> _friendlyName;

        public MethodDescriptor(TypeDescriptor typeDescriptor,
            MethodInfo methodInfo, ITypeCache typeCache) :
            base(methodInfo.Name, methodInfo.ToLazy(x => x
                .GetCustomAttributes().ToArray()))
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
    }

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

    public class PropertyDescriptor : DescriptorBase
    {
        private readonly Lazy<TypeDescriptor> _propertyType;

        public PropertyDescriptor(TypeDescriptor typeDescriptor,
            PropertyInfo propertyInfo, ITypeCache typeCache) :
            base(propertyInfo.Name, propertyInfo.ToLazy(
                    x => x.GetCustomAttributes().ToArray()))
        {
            DeclaringType = typeDescriptor;
            PropertyInfo = propertyInfo;
            _propertyType = propertyInfo.ToLazy(x => typeCache.GetTypeDescriptor(x.PropertyType));
        }

        public TypeDescriptor DeclaringType { get; }
        public TypeDescriptor PropertyType => _propertyType.Value;
        public PropertyInfo PropertyInfo { get; }
    }

    public abstract class DescriptorBase
    {
        private readonly Lazy<Attribute[]> _attributes;

        protected DescriptorBase(string name, Lazy<Attribute[]> attributes)
        {
            _attributes = attributes;
            Name = name;
        }

        public string Name { get; }
        public Attribute[] Attributes => _attributes.Value;

        public bool HasAttribute<T>() where T : Attribute
        {
            return GetAttributes<T>().Any();
        }

        public bool HasAnyAttribute<T1, T2>() where T1 : Attribute where T2 : Attribute
        {
            return GetAttributes<T1>().Any() || GetAttributes<T2>().Any();
        }

        public T GetAttribute<T>() where T : Attribute
        {
            return GetAttributes<T>().FirstOrDefault();
        }

        public T[] GetAttributes<T>() where T : Attribute
        {
            return Attributes.OfType<T>().ToArray();
        }
    }
}