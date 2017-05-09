using Graphite.Extensions;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Graphite.Reflection;

namespace Graphite.Actions
{
    public class ActionMethod
    {
        public ActionMethod(TypeDescriptor handlerTypeDescriptor, MethodDescriptor methodDescriptor)
        {
            FullName = $"{handlerTypeDescriptor.FriendlyFullName}.{methodDescriptor.FriendlyName}";
            HandlerTypeDescriptor = handlerTypeDescriptor;
            MethodDescriptor = methodDescriptor;
            Invoke = methodDescriptor.GenerateAsyncInvoker(handlerTypeDescriptor);
        }

        public virtual string FullName { get; }
        public virtual TypeDescriptor HandlerTypeDescriptor { get; }
        public virtual MethodDescriptor MethodDescriptor { get; }
        public virtual Func<object, object[], Task<object>> Invoke { get; }

        public override int GetHashCode()
        {
            return this.GetHashCode(HandlerTypeDescriptor, MethodDescriptor);
        }

        public override bool Equals(object obj)
        {
            return GetHashCode() == obj?.GetHashCode();
        }

        public override string ToString()
        {
            return FullName;
        }

        public static ActionMethod From<T>(Expression<Action<T>> method, ITypeCache typeCache = null)
        {
            return From((LambdaExpression)method, typeCache);
        }

        public static ActionMethod From<T>(Expression<Func<T, object>> method, ITypeCache typeCache = null)
        {
            return From((LambdaExpression)method, typeCache);
        }

        public static ActionMethod From(LambdaExpression method, ITypeCache typeCache = null)
        {
            return From(method.GetMethodInfo(), typeCache);
        }

        public static ActionMethod From(MethodInfo method, ITypeCache typeCache = null)
        {
            typeCache = typeCache ?? new TypeCache();
            var typeDescriptor = new TypeDescriptor(method.DeclaringType, typeCache);
            return new ActionMethod(typeDescriptor, new MethodDescriptor(
                typeDescriptor, method, typeCache));
        }
    }
}