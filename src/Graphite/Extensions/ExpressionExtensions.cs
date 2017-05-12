using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Graphite.Reflection;

namespace Graphite.Extensions
{
    public static class StaticMethod
    {
        public static MethodCallExpression Call(Expression arg1,
            Expression<Func<object>> method, params Type[] typeArguments)
        {
            return Expression.Call(null, Info(method, typeArguments), arg1);
        }

        public static MethodCallExpression Call(
            Expression<Func<object>> method, Type typeArg1,
            params Expression[] arguments)
        {
            return Expression.Call(null, Info(method, typeArg1), arguments);
        }

        public static MethodInfo Info(Expression<Func<object>> method, params Type[] typeArguments)
        {
            return method.GetMethodInfo(typeArguments);
        }
    }

    public class Type<T>
    {
        public static MethodCallExpression Call(Expression instance,
            Expression<Func<T, object>> method, params Expression[] parameters)
        {
            return System.Linq.Expressions.Expression.Call(instance, Method(method), parameters);
        }

        public static MethodCallExpression Call(Expression instance,
            Expression<Func<T, object>> method, Type typeArg1, 
            params Expression[] parameters)
        {
            return System.Linq.Expressions.Expression.Call(instance, method.GetMethodInfo(typeArg1), parameters);
        }

        public static MethodInfo Method(Expression<Func<T, object>> expression)
        {
            return (expression.UnwrapConvert() as MethodCallExpression)?.Method;
        }

        public static MethodInfo Method(Expression<Action<T>> expression)
        {
            return (expression.UnwrapConvert() as MethodCallExpression)?.Method;
        }

        public static Expression<Func<T, object>> Expression(Expression<Func<T, object>> expression)
        {
            return expression;
        }

        public static Expression<Action<T>> Expression(Expression<Action<T>> expression)
        {
            return expression;
        }

        public static PropertyInfo Property(Expression<Func<T, object>> property)
        {
            return (property.UnwrapConvert() as MemberExpression)?.Member as PropertyInfo;
        }

        public static ParameterExpression Parameter(string name = "")
        {
            return System.Linq.Expressions.Expression.Parameter(typeof(T), name);
        }
    }

    public static class ExpressionExtensions
    {
        public static MethodInfo GetMethodInfo(this LambdaExpression method, 
            params Type[] typeArguments)
        {
            var methodInfo = (method.UnwrapConvert() as MethodCallExpression)?.Method;
            if (methodInfo == null)
                throw new Exception("Expression does not contain a method call.");
            if (!typeArguments.Any() || !methodInfo.IsGenericMethod) return methodInfo;
            if (!methodInfo.IsGenericMethodDefinition)
                methodInfo = methodInfo.GetGenericMethodDefinition();
            return methodInfo.MakeGenericMethod(typeArguments);
        }

        public static Expression UnwrapConvert(this LambdaExpression expression)
        {
            return expression.Body.NodeType == ExpressionType.Convert ?
                ((UnaryExpression)expression.Body).Operand : expression.Body;
        }
        
        public static Expression InForEach(this Expression collection, 
            ParameterExpression loopVar, Expression body)
        {
            var elementType = loopVar.Type;
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(elementType);
            var enumerator = typeof(IEnumerator<>).MakeGenericType(elementType).Variable();
            var getEnumeratorCall = Expression.Call(collection, enumerableType
                .GetMethod(nameof(IEnumerable.GetEnumerator)));
            var moveNextCall = Expression.Call(enumerator, Type<IEnumerator>.Method(x => x.MoveNext()));
            var breakLabel = Expression.Label("LoopBreak");

            return Expression.Block(new[] { enumerator },
                enumerator.Assign(getEnumeratorCall),
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.Equal(moveNextCall, true.ToConstant()),
                        Expression.Block(new[] { loopVar },
                            loopVar.Assign(enumerator.PropertyAccess(nameof(IEnumerator.Current))),
                            body
                        ),
                        Expression.Break(breakLabel)
                    ),
                breakLabel)
            );
        }

        public static ParameterExpression ToParameter(this Type type)
        {
            return Expression.Parameter(type);
        }

        public static ConstantExpression ToConstant(this object constant)
        {
            return Expression.Constant(constant);
        }

        public static UnaryExpression Convert<T>(this Expression expression)
        {
            return expression.Convert(typeof(T));
        }

        public static UnaryExpression Convert(this Expression expression, Type type)
        {
            return Expression.Convert(expression, type);
        }

        public static InvocationExpression Invoke(this Delegate @delegate,
            params Expression[] parameters)
        {
            return Expression.Invoke(Expression.Constant(@delegate), parameters);
        }

        public static LambdaExpression ToLambda(this Expression expression,
            params ParameterExpression[] parameters)
        {
            return Expression.Lambda(expression, parameters);
        }

        public static InvocationExpression InvokeLambda(this Expression lambda,
            params Expression[] parameters)
        {
            return Expression.Invoke(lambda, parameters);
        }

        public static MethodCallExpression Call(this MethodInfo method,
            params Expression[] parameters)
        {
            return Expression.Call(null, method, parameters);
        }

        public static Expression Call<T>(this Expression instance,
            Expression<Func<T, object>> method, params Expression[] parameters)
        {
            return Type<T>.Call(instance, method, parameters);
        }

        public static Expression ToArray<T>(this Expression source)
        {
            return source.ToArray(typeof(T));
        }

        public static Expression ToArray(this Expression source, Type itemType)
        {
            return StaticMethod.Info(() => Enumerable.ToArray<object>(null), itemType).Call(source);
        }

        public static Expression ToList(this Expression source, Type itemType)
        {
            return StaticMethod.Info(() => Enumerable.ToList<object>(null), itemType).Call(source);
        }

        public static Expression FirstOrDefault(this Expression source, Type itemType)
        {
            return StaticMethod.Info(() => Enumerable.FirstOrDefault<object>(null), itemType).Call(source);
        }

        public static Expression MemberAccess<T>(this Expression expression, Expression<Func<T, object>> member)
        {
            return Expression.MakeMemberAccess(expression, Type<T>.Property(member));
        }

        public static Expression MemberAccess(this Expression expression, MemberInfo member)
        {
            return Expression.MakeMemberAccess(expression, member);
        }

        public static Expression PropertyAccess(this Expression expression, string name)
        {
            return Expression.Property(expression, name);
        }

        public static Expression PropertyAccess(this Expression expression, PropertyInfo property)
        {
            return Expression.Property(expression, property);
        }

        public static Expression Assign(this Expression left, Expression right)
        {
            return Expression.Assign(left, right);
        }

        public static Expression If(this Expression ifTrue, Expression test)
        {
            return Expression.IfThen(test, ifTrue);
        }

        public static ParameterExpression Variable(this Type type)
        {
            return Expression.Variable(type);
        }
    }
}
