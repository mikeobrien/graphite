using System;
using Graphite.Reflection;

namespace Graphite.Routing
{
    public class ActionParameter
    {
        private readonly DescriptorBase _descriptor;

        public ActionParameter(ParameterDescriptor parameterDescriptor)
        {
            _descriptor = parameterDescriptor;
            Name = parameterDescriptor.Name;
            IsParameter = true;
            ParameterDescriptor = parameterDescriptor;
            TypeDescriptor = parameterDescriptor?.ParameterType;
        }

        public ActionParameter(ParameterDescriptor parameterDescriptor, 
            PropertyDescriptor property) : this(property)
        {
            IsPropertyOfParameter = true;
            ParameterDescriptor = parameterDescriptor;
        }

        public ActionParameter(PropertyDescriptor propertyDescriptor)
        {
            _descriptor = propertyDescriptor;
            Name = propertyDescriptor.Name;
            IsProperty = true;
            PropertyDescriptor = propertyDescriptor;
            TypeDescriptor = propertyDescriptor?.PropertyType;
        }

        public ActionParameter(ActionParameter parameter)
        {
            _descriptor = parameter._descriptor;
            Name = parameter.Name;
            TypeDescriptor = parameter.TypeDescriptor;
            IsParameter = parameter.IsParameter;
            ParameterDescriptor = parameter.ParameterDescriptor;
            IsProperty = parameter.IsProperty;
            PropertyDescriptor = parameter.PropertyDescriptor;
        }

        public string Name { get; }
        public virtual TypeDescriptor TypeDescriptor { get; }
        public virtual bool IsParameter { get; }
        public virtual ParameterDescriptor ParameterDescriptor { get; }
        public virtual bool IsPropertyOfParameter { get; }
        public virtual bool IsProperty { get; }
        public virtual PropertyDescriptor PropertyDescriptor { get; }

        public Attribute[] Attributes => _descriptor.Attributes;
        public bool HasAttribute<T>() where T : Attribute => _descriptor.HasAttribute<T>();
        public bool HasAttributes<T1, T2>() where T1 : Attribute where T2 : Attribute => 
            _descriptor.HasAttributes<T1, T2>();
        public T GetAttribute<T>() where T : Attribute => _descriptor.GetAttribute<T>();
        public T[] GetAttributes<T>() where T : Attribute => _descriptor.GetAttributes<T>();

        public override bool Equals(object obj)
        {
            var actionParameter = obj as ActionParameter;
            return actionParameter != null && (IsParameter 
                ? ParameterDescriptor.Equals(actionParameter.ParameterDescriptor) 
                : PropertyDescriptor.Equals(actionParameter.PropertyDescriptor));
        }

        public override int GetHashCode()
        {
            return IsParameter ? ParameterDescriptor.GetHashCode() : PropertyDescriptor.GetHashCode();
        }
    }
}