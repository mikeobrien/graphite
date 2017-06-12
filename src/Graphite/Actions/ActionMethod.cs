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
        public virtual string Name => MethodDescriptor.Name;
        public virtual TypeDescriptor HandlerTypeDescriptor { get; }
        public virtual MethodDescriptor MethodDescriptor { get; }
        public virtual Func<object, object[], Task<object>> Invoke { get; }
        public Attribute[] Attributes => MethodDescriptor.Attributes;

        public bool HasAttribute<T>() where T : Attribute
        {
            return MethodDescriptor.HasAttribute<T>();
        }

        public bool HasAttributes<T1, T2>() where T1 : Attribute where T2 : Attribute
        {
            return MethodDescriptor.HasAttributes<T1, T2>();
        }

        public T GetAttribute<T>() where T : Attribute
        {
            return MethodDescriptor.GetAttribute<T>();
        }

        public T[] GetAttributes<T>() where T : Attribute
        {
            return MethodDescriptor.GetAttributes<T>();
        }

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