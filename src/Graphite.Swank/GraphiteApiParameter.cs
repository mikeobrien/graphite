using System;
using System.Collections.Generic;
using System.Reflection;
using Graphite.Reflection;
using Graphite.Routing;
using Swank.Description;

namespace Graphite.Swank
{
    public class GraphiteApiParameter : IApiParameterDescription
    {
        private readonly DescriptorBase _descriptor;

        public GraphiteApiParameter(ActionParameter actionParameter, 
            bool urlParameter = false, bool querystring = false)
            :this(actionParameter.Descriptor, actionParameter.Name, 
                actionParameter.TypeDescriptor, actionParameter.Action.MethodDescriptor, 
                urlParameter, querystring) { }

        public GraphiteApiParameter(ParameterDescriptor parameterDescriptor)
            :this(parameterDescriptor, parameterDescriptor.Name, 
                parameterDescriptor.ParameterType, parameterDescriptor.Method) { }

        private GraphiteApiParameter(DescriptorBase descriptor, 
            string name, TypeDescriptor type, MethodDescriptor method, 
            bool urlParameter = false, bool querystring = false)
        {
            _descriptor = descriptor;
            Name = name;
            Type = type.Type;
            ActionMethod = method.MethodInfo;
            IsUrlParameter = urlParameter;
            IsQuerystring = querystring;
            IsOptional = querystring && (type.IsNullable || !Type.IsValueType);
        }

        public string Name { get; }
        public string Documentation { get; } = null;
        public object DefaultValue { get; } = null;
        public Type Type { get; }
        public bool IsOptional { get; }
        public bool IsUrlParameter { get; }
        public bool IsQuerystring { get; }
        public MethodInfo ActionMethod { get; }

        public T GetAttribute<T>() where T : Attribute =>
            _descriptor.GetAttribute<T>();

        public IEnumerable<T> GetAttributes<T>() where T : Attribute =>
            _descriptor.GetAttributes<T>();

        public bool HasAttribute<T>() where T : Attribute =>
            _descriptor.HasAttribute<T>();
    }
}