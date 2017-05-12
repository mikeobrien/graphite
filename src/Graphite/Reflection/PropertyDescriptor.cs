using System;
using System.Linq;
using System.Reflection;
using Graphite.Extensions;

namespace Graphite.Reflection
{
    public class PropertyDescriptor : DescriptorBase
    {
        private readonly Lazy<TypeDescriptor> _propertyType;
        private readonly Lazy<Action<object, object>> _setValue;
        private readonly Lazy<Func<object, object>> _getValue;

        public PropertyDescriptor(TypeDescriptor typeDescriptor,
            PropertyInfo propertyInfo, ITypeCache typeCache) :
            base(propertyInfo.Name, propertyInfo.ToLazy(
                x => x.GetCustomAttributes().ToArray()))
        {
            DeclaringType = typeDescriptor;
            PropertyInfo = propertyInfo;
            _propertyType = propertyInfo.ToLazy(x => typeCache.GetTypeDescriptor(x.PropertyType));
            _setValue = propertyInfo.ToLazy(x => x.CompileSetter());
            _getValue = propertyInfo.ToLazy(x => x.CompileGetter());
        }

        public TypeDescriptor DeclaringType { get; }
        public TypeDescriptor PropertyType => _propertyType.Value;
        public PropertyInfo PropertyInfo { get; }

        public void SetValue(object instance, object value)
        {
            _setValue.Value(instance, value);
        }

        public object GetValue(object instance)
        {
            return _getValue.Value(instance);
        }
    }
}