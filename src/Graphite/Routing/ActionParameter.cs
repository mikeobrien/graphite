using System;
using Graphite.Actions;
using Graphite.Reflection;
using Graphite.Extensions;

namespace Graphite.Routing
{
    public class ActionParameter
    {
        public ActionParameter(ActionMethod action, ParameterDescriptor parameter)
        {
            Descriptor = parameter;
            Name = parameter.Name;
            IsParameter = true;
            Action = action;
            ParameterDescriptor = parameter;
            TypeDescriptor = parameter?.ParameterType;
        }

        public ActionParameter(ActionMethod action, ParameterDescriptor parameter, 
            PropertyDescriptor property)
        {
            Descriptor = property;
            Name = property.Name;
            IsPropertyOfParameter = true;
            Action = action;
            ParameterDescriptor = parameter;
            PropertyDescriptor = property;
            TypeDescriptor = property?.PropertyType;
        }

        public ActionParameter(ActionMethod action, PropertyDescriptor property)
        {
            Descriptor = property;
            Name = property.Name;
            IsProperty = true;
            Action = action;
            PropertyDescriptor = property;
            TypeDescriptor = property?.PropertyType;
        }

        public ActionParameter(ActionParameter actionParameter)
        {
            Descriptor = actionParameter.Descriptor;
            Name = actionParameter.Name;
            Action = actionParameter.Action;
            TypeDescriptor = actionParameter.TypeDescriptor;
            IsParameter = actionParameter.IsParameter;
            ParameterDescriptor = actionParameter.ParameterDescriptor;
            IsProperty = actionParameter.IsProperty;
            PropertyDescriptor = actionParameter.PropertyDescriptor;
        }

        public DescriptorBase Descriptor { get; }
        public virtual string Name { get; }
        public virtual ActionMethod Action { get; set; }
        public virtual TypeDescriptor TypeDescriptor { get; }
        public virtual bool IsParameter { get; }
        public virtual ParameterDescriptor ParameterDescriptor { get; }
        public virtual bool IsPropertyOfParameter { get; }
        public virtual bool IsProperty { get; }
        public virtual PropertyDescriptor PropertyDescriptor { get; }

        public Attribute[] Attributes => Descriptor.Attributes;
        public bool HasAttribute<T>() where T : Attribute => Descriptor.HasAttribute<T>();
        public bool HasAttributes<T1, T2>() where T1 : Attribute where T2 : Attribute => 
            Descriptor.HasAttributes<T1, T2>();
        public T GetAttribute<T>() where T : Attribute => Descriptor.GetAttribute<T>();
        public T[] GetAttributes<T>() where T : Attribute => Descriptor.GetAttributes<T>();

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