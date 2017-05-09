using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Graphite.Reflection;
using TaskExtensions = Graphite.Extensions.TaskExtensions;

namespace Graphite.Actions
{
    public static class AsyncInvoker
    {
        public static Func<object, object[], Task<object>> GenerateAsyncInvoker(
            this MethodDescriptor method, TypeDescriptor instanceType)
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            var parameters = Expression.Parameter(typeof(object[]), "params");
            var arguments = method.Parameters.Select(x => Expression.Convert(Expression
                .ArrayAccess(parameters, Expression.Constant(x.Position)), 
                    x.ParameterType.Type));
            var callMethod = Expression.Call(Expression.Convert(
                instance, instanceType.Type), method.MethodInfo, arguments);

            Expression result;

            if (method.HasResult)
            {
                if (method.ReturnType.IsTask)
                    result = callMethod.ConvertToObjectReturn();
                else if (method.ReturnType.IsTaskWithResult)
                    result = callMethod.ConvertToObjectReturn(
                        method.ReturnType.Type);
                else
                    result = Expression.Convert(callMethod, typeof(object)).FromTaskResult();
            }
            else result = Expression.Block(callMethod, Expression.Constant(null).FromTaskResult());

            return Expression.Lambda<Func<object, object[], Task<object>>>(
                result, instance, parameters).Compile();
        }

        private static Expression ConvertToObjectReturn(this Expression expression)
        {
            return Expression.Call(typeof(TaskExtensions), nameof(TaskExtensions
                .ConvertToObjectReturn), new Type[] { }, expression);
        }

        private static Expression ConvertToObjectReturn(this Expression expression, Type returnType)
        {
            return Expression.Call(typeof(TaskExtensions), nameof(TaskExtensions
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
