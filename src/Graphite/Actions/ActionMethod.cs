using Graphite.Extensions;
using System;
using System.Threading.Tasks;
using Graphite.Reflection;

namespace Graphite.Actions
{
    public class ActionMethod
    {
        public ActionMethod(TypeDescriptor handlerType, MethodDescriptor method)
        {
            FullName = $"{handlerType.FriendlyFullName}.{method.FriendlyName}";
            HandlerType = handlerType;
            Method = method;
            Invoke = method.GenerateAsyncInvoker(handlerType);
        }

        public virtual string FullName { get; }
        public virtual TypeDescriptor HandlerType { get; }
        public virtual MethodDescriptor Method { get; }
        public virtual Func<object, object[], Task<object>> Invoke { get; }

        public override int GetHashCode()
        {
            return this.GetHashCode(HandlerType, Method);
        }

        public override bool Equals(object obj)
        {
            return GetHashCode() == obj?.GetHashCode();
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}