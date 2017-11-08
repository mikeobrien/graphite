using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Graphite.Extensions;
using Graphite.Reflection;

namespace Graphite.Actions
{
    public class ParameterInvalidCastException : Exception
    {
        public ParameterInvalidCastException(object value, ParameterDescriptor parameter, 
            Exception exception) : base(GetMessage(value, parameter), exception) { }

        private static string GetMessage(object value, ParameterDescriptor parameter)
        {
            return $"Unable to cast {parameter.Name} {value?.GetType().Name} argument " +
                   $"to {parameter.ParameterType.Type.Name}.";
        }
    }

    public static class AsyncInvoker
    {
        private static readonly ConstructorInfo ParameterInvalidCastExceptionCtor =
            typeof(ParameterInvalidCastException).GetConstructors().First();
        public static Func<object, object[], Task<object>> GenerateAsyncInvoker(
            this MethodDescriptor method, TypeDescriptor instanceType)
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            var parameters = Expression.Parameter(typeof(object[]), "params");
            var variables = method.Parameters.Select(x => Expression.Variable(x.ParameterType.Type)).ToArray();
            var body = new List<Expression>();

            body.AddRange(method.Parameters.Zip(variables, (p, v) => new
                {
                    Parameter = p,
                    Variable = v,
                    CatchParameter = Expression.Parameter(typeof(Exception)),
                    Value = Expression.ArrayAccess(parameters, Expression.Constant(p.Position))
                })
                .Select(x => Expression.TryCatch(
                    Expression.Block(typeof(void), 
                        x.Parameter.ParameterType.Type.IsValueType 
                            ? x.Variable.Assign(x.Value.Coalesce(x.Parameter.ParameterType.Type.ToDefault()
                                .Convert<object>()).Convert(x.Parameter.ParameterType.Type))
                            : x.Variable.Assign(x.Value.Convert(x.Parameter.ParameterType.Type))),
                    Expression.Catch(x.CatchParameter, Expression.Throw(
                        ParameterInvalidCastExceptionCtor.ToNew(
                            x.Value, x.Parameter.ToConstant(), x.CatchParameter))))));

            var callMethod = Expression.Call(Expression.Convert(
                instance, instanceType.Type), method.MethodInfo, variables);

            if (method.HasResult)
            {
                if (method.ReturnType.IsTask)
                    body.Add(callMethod.ConvertToObjectReturn());
                else if (method.ReturnType.IsTaskWithResult)
                    body.Add(callMethod.ConvertToObjectReturn(
                        method.ReturnType.Type));
                else
                    body.Add(Expression.Convert(callMethod, typeof(object)).FromTaskResult());
            }
            else
            {
                body.Add(callMethod);
                body.Add(Expression.Constant(null).FromTaskResult());
            }

            return Expression.Lambda<Func<object, object[], Task<object>>>(
                Expression.Block(variables, body.ToArray()), instance, parameters).Compile();
        }

        private static Expression ConvertToObjectReturn(this Expression expression)
        {
            return Expression.Call(typeof(ThreadingExtensions), nameof(ThreadingExtensions
                .ConvertToObjectReturn), new Type[] { }, expression);
        }

        private static Expression ConvertToObjectReturn(this Expression expression, Type returnType)
        {
            return Expression.Call(typeof(ThreadingExtensions), nameof(ThreadingExtensions
                .ConvertToObjectReturn), new[] { returnType }, expression);
        }

        private static Expression FromTaskResult(this Expression expression)
        {
            return expression.FromTaskResult<object>();
        }

        private static Expression FromTaskResult<T>(this Expression expression)
        {
            return Expression.Call(typeof(Task), nameof(Task.FromResult),
                new[] { typeof(T) }, expression);
        }
    }
}
