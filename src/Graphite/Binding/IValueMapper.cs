using System;
using Graphite.Actions;
using Graphite.Extensibility;
using Graphite.Reflection;

namespace Graphite.Binding
{
    public class ValueMapperContext
    {
        public ValueMapperContext(
            Configuration configuration, RequestContext requestContext, 
            ParameterDescriptor parameter, object[] values) :
            this(configuration, requestContext, parameter?.ParameterType, values)
        {
            IsParameter = true;
            Parameter = parameter;
        }

        public ValueMapperContext(
            Configuration configuration, RequestContext requestContext,
                PropertyDescriptor property, object[] values) : 
            this(configuration, requestContext, property?.PropertyType, values)
        {
            IsProperty = true;
            Property = property;
        }

        private ValueMapperContext(Configuration configuration, RequestContext requestContext,
            TypeDescriptor type, object[] values)
        {
            Configuration = configuration;
            RequestContext = requestContext;
            Type = type;
            Values = values;
        }

        public virtual Configuration Configuration { get; }
        public virtual RequestContext RequestContext { get; }
        public virtual TypeDescriptor Type { get; }
        public virtual bool IsParameter { get; }
        public virtual ParameterDescriptor Parameter { get; }
        public virtual bool IsProperty { get; }
        public virtual PropertyDescriptor Property { get; }
        public virtual object[] Values { get; }
    }

    public interface IValueMapper : IConditional<ValueMapperContext>
    {
        object Map(ValueMapperContext context);
    }
}