using System;
using Graphite.Linq;

namespace Graphite.Actions
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ResponseHeaderAttribute : Attribute
    {
        public ResponseHeaderAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }
        public string Value { get; }
    }

    public class AttributeResponseHeaders : IResponseHeaders
    {
        private readonly ActionMethod _actionMethod;

        public AttributeResponseHeaders(ActionMethod actionMethod)
        {
            _actionMethod = actionMethod;
        }

        public static bool AppliesTo(ActionConfigurationContext context)
        {
            return context.ActionMethod.HasAttribute<ResponseHeaderAttribute>();
        }

        public virtual bool AppliesTo(ResponseHeadersContext context)
        {
            return true;
        }

        public void SetHeaders(ResponseHeadersContext context)
        {
            _actionMethod.GetAttributes<ResponseHeaderAttribute>()
                .ForEach(x => context.ResponseMessage.Headers.Add(x.Name, x.Value));
        }
    }
}