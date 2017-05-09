using System;
using System.Linq;
using Graphite.Reflection;
using Graphite.Extensions;

namespace Graphite.Binding
{
    public class SimpleTypeMapper : ValueMapperBase
    {
        public override bool AppliesTo(ValueMapperContext context)
        {
            return (context.Type.ElementType ?? context.Type).Type.IsSimpleType();
        }
        
        public override object Map<T>(ValueMapperContext context)
        {
            try
            {
                var type = context.Type.ElementType ?? context.Type;
                var result = context.Values.Select(x => (T)(x is string
                    ? x.ToString().ParseSimpleType(type) : x));

                return context.Type.IsArray
                    ? result.ToArray()
                    : (context.Type.IsGenericListCastable
                        ? (object)result.ToList()
                        : result.FirstOrDefault());
            }
            catch (FormatException exception)
            {
                throw new BadRequestException($@"Parameter {context.Parameter.Name} value {context.Values
                    .Select(x => $"'{x}'").Join(",")} is not formatted correctly. {exception.Message}", exception);
            }
        }
    }
}
