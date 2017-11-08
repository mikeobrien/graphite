using System;
using System.Linq;
using Graphite.Reflection;
using Graphite.Extensions;

namespace Graphite.Binding
{
    public class SimpleTypeMapper : ValueMapperBase
    {
        public SimpleTypeMapper(Configuration configuration) : base(configuration) { }

        public override bool AppliesTo(ValueMapperContext context)
        {
            return (context.Type.ElementType ?? context.Type).Type.IsSimpleType();
        }
        
        public override MapResult Map<T>(ValueMapperContext context)
        {
            try
            {
                var type = context.Type.ElementType ?? context.Type;
                var result = context.Values.Select(x => (T)(x is string
                    ? x.ToString().ParseSimpleType(type) : x));

                var value = context.Type.IsArray
                    ? result.ToArray()
                    : (context.Type.IsGenericListCastable
                        ? (object)result.ToList()
                        : result.FirstOrDefault());

                return MapResult.Success(value);
            }
            catch (FormatException exception)
            {
                var message = $"{context.Values.Select(x => $"'{x}'").Join(",")} " +
                    $"is not formatted correctly. {exception.Message}";

                return MapResult.Failure(context.Parameter != null
                    ? $"Parameter {context.Parameter.Name} value {message}" 
                    : $"Value {message}");
            }
        }
    }
}
