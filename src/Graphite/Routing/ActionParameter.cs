using System;
using Graphite.Actions;
using Graphite.Reflection;
using Graphite.Extensions;

namespace Graphite.Routing
{
    public class ActionParameter
    {
        private readonly DescriptorBase _descriptor;

        public ActionParameter(ActionMethod action, ParameterDescriptor parameter)
        {
            _descriptor = parameter;
            Name = parameter.Name;
            IsParameter = true;
            Action = action;
            ParameterDescriptor = parameter;
            TypeDescriptor = parameter?.ParameterType;
        }

        public ActionParameter(ActionMethod action, ParameterDescriptor parameter, 
            PropertyDescriptor property)
        {
            _descriptor = property;
            Name = property.Name;
            IsPropertyOfParameter = true;
            Action = action;
            ParameterDescriptor = parameter;
            PropertyDescriptor = property;
            TypeDescriptor = property?.PropertyType;
        }

        public ActionParameter(ActionMethod action, PropertyDescriptor property)
        {
            _descriptor = property;
            Name = property.Name;
            IsProperty = true;
            Action = action;
            PropertyDescriptor = property;
            TypeDescriptor = property?.PropertyType;
        }

        public ActionParameter(ActionParameter actionParameter)
        {
            _descriptor = actionParameter._descriptor;
            Name = actionParameter.Name;
            Action = actionParameter.Action;
            TypeDescriptor = actionParameter.TypeDescriptor;
            IsParameter = actionParameter.IsParameter;
            ParameterDescriptor = actionParameter.ParameterDescriptor;
            IsProperty = actionParameter.IsProperty;
            PropertyDescriptor = actionParameter.PropertyDescriptor;
        }

        public string Name { get; }
        public ActionMethod Action { get; set; }
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

        public override int GetHashCode()
        {
            return this.GetHashCode(IsParameter ? ParameterDescriptor : 
                (DescriptorBase)PropertyDescriptor);
        }

        public override bool Equals(object obj)
        {
            return GetHashCode() == obj?.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}