using System;
using System.Reflection;
using Graphite.Reflection;
using Swank.Description;

namespace Graphite.Swank
{
    public class GraphiteApiParameter : IParameterDescription
    {
        private readonly ParameterDescriptor _parameterDescriptor;
        
        public GraphiteApiParameter(ParameterDescriptor parameterDescriptor, 
            bool urlParameter = false, bool querystring = false)
        {
            _parameterDescriptor = parameterDescriptor;
            IsUrlParameter = urlParameter;
            IsQuerystring = querystring;
            IsOptional = querystring &&
                         (_parameterDescriptor.ParameterType.IsNullable || 
                          !_parameterDescriptor.ParameterType.Type.IsValueType);
        }

        public string Name => _parameterDescriptor.Name;
        public string Documentation { get; } = null;
        public object DefaultValue { get; } = null;
        public Type Type => _parameterDescriptor.ParameterType.Type;
        public bool IsOptional { get; }
        public bool IsUrlParameter { get; }
        public bool IsQuerystring { get; }
        public MethodInfo ActionMethod => _parameterDescriptor.Method.MethodInfo;

        public T GetAttribute<T>() where T : Attribute =>
            _parameterDescriptor.GetAttribute<T>();

        public bool HasAttribute<T>() where T : Attribute =>
            _parameterDescriptor.HasAttribute<T>();
    }
}