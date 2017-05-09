using System;
using System.Linq;
using System.Linq.Expressions;
using Graphite.Extensions;
using Graphite.Reflection;

namespace Graphite.Binding
{
    public static class PropertyBinder
    {
        private static readonly Func<Type, TypeDescriptor, 
            Func<object, ILookup<string, object>,
            Func<PropertyDescriptor, object[], MapResult>, object>> Binders =
                Memoize.Func<Type, TypeDescriptor,
                    Func<object, ILookup<string, object>,
                    Func<PropertyDescriptor, object[], MapResult>, object>>((t, td) => GetBinder(td));

        public static object CreateAndBind(this TypeDescriptor type,
            ILookup<string, object> values,
            Func<PropertyDescriptor, object[], MapResult> map)
        {
            return Binders(type.Type, type)(null, values, map);
        }

        public static Func<object, ILookup<string, object>,
            Func<PropertyDescriptor, object[], MapResult>, object> 
            GetBinder(TypeDescriptor type)
        {
            var properties = type.Properties;

            var instance = Type<object>.Parameter();
            var values = Type<ILookup<string, object>>.Parameter();
            var value = Type<IGrouping<string, object>>.Parameter();
            var map = Type<Func<PropertyDescriptor, object[], MapResult>>.Parameter();
            var mapResult = Type<MapResult>.Parameter();

            var ensureInstance = Expression.IfThen(
                Expression.Equal(instance, (null as object).ToConstant()),
                Expression.Assign(instance, type.Type.New()));

            var setValues = properties.Aggregate((Expression)Expression.Empty(), (@else, x) =>
            {
                return Expression.IfThenElse(

                    value.MemberAccess<IGrouping<string, object>>(y => y.Key)
                        .Call<string>(y => y.Equals(null, StringComparison.Ordinal),
                            x.Name.ToConstant(), StringComparison.OrdinalIgnoreCase.ToConstant()),

                    Expression.Block(
                        new [] { mapResult },
                        mapResult.Assign(map.InvokeLambda(x.ToConstant(), value.ToArray<object>())),

                        Expression.IfThen(
                            mapResult.MemberAccess<MapResult>(y => y.Mapped),
                        
                            instance.Convert(type.Type).MemberAccess(x.PropertyInfo).Assign(mapResult
                                .MemberAccess<MapResult>(y => y.Value).Convert(x.PropertyType.Type)))), 

                    @else);
            });
            var body = values.InForEach(value, Expression.Block(setValues));
            return Expression.Lambda<Func<object, ILookup<string, object>,
                    Func<PropertyDescriptor, object[], MapResult>, object>>(
                Expression.Block(ensureInstance, body, instance), 
                    instance, values, map).Compile();
        }
    }
}
