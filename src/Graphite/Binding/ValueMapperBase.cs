using System;
using System.Linq.Expressions;
using Graphite.Extensions;

namespace Graphite.Binding
{
    public abstract class ValueMapperBase : IValueMapper
    {
        public abstract bool AppliesTo(ValueMapperContext context);
        public abstract MapResult Map<T>(ValueMapperContext context);

        private static readonly Func<Type, Func<ValueMapperBase, ValueMapperContext, MapResult>> Mappers =
            Memoize.Func<Type, Func<ValueMapperBase, ValueMapperContext, MapResult>>(GetMapper);

        public MapResult Map(ValueMapperContext context)
        {
            return Mappers(context.Type.ElementType?.Type ?? context.Type.Type)(this, context);
        }

        private static Func<ValueMapperBase, ValueMapperContext, MapResult> GetMapper(Type type)
        {
            var instance = Type<ValueMapperBase>.Parameter();
            var context = Type<ValueMapperContext>.Parameter();
            return Expression.Lambda<Func<ValueMapperBase, ValueMapperContext, MapResult>>(
                Type<ValueMapperBase>.Call(instance, x => x.Map<object>(null), type, context), 
                    instance, context).Compile();
        }
    }
}
