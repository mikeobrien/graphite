using System;
using Graphite.Actions;
using Graphite.Extensibility;
using Graphite.Reflection;
using Graphite.Routing;

namespace Graphite.Binding
{
    public class ValueMapperContext
    {
        private readonly ActionParameter _parameter;

        public ValueMapperContext(
            Configuration configuration, RequestContext requestContext,
            ActionParameter parameter, object[] values)
        {
            _parameter = parameter;
            Configuration = configuration;
            RequestContext = requestContext;
            Values = values;
        }

        public virtual Configuration Configuration { get; }
        public virtual RequestContext RequestContext { get; }
        public virtual string Name => _parameter.Name;
        public virtual TypeDescriptor Type => _parameter.TypeDescriptor;
        public virtual bool ForParameter => _parameter.IsParameter;
        public virtual ParameterDescriptor Parameter => _parameter.ParameterDescriptor;
        public virtual bool ForPropertyOfParameter => _parameter.IsPropertyOfParameter;
        public virtual bool ForProperty => _parameter.IsProperty;
        public virtual PropertyDescriptor Property => _parameter.PropertyDescriptor;
        public virtual object[] Values { get; }

        public Attribute[] Attributes => _parameter.Attributes;
        public bool HasAttribute<T>() where T : Attribute => _parameter.HasAttribute<T>();
        public bool HasAnyAttribute<T1, T2>() where T1 : Attribute where T2 : Attribute =>
            _parameter.HasAttributes<T1, T2>();
        public T GetAttribute<T>() where T : Attribute => _parameter.GetAttribute<T>();
        public T[] GetAttributes<T>() where T : Attribute => _parameter.GetAttributes<T>();
    }

    public interface IValueMapper : IConditional<ValueMapperContext>
    {
        object Map(ValueMapperContext context);
    }
}