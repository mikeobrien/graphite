using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Graphite.Extensions;
using Graphite.Routing;

namespace Graphite.Binding
{
    public class FromHeadersAttribute : Attribute
    {
        public FromHeadersAttribute() { }

        public FromHeadersAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    public class HeaderBinder : ParameterBinderBase
    {
        public const string HeaderPostfix = "Header";

        public HeaderBinder(IEnumerable<IValueMapper> mappers) : base(mappers) { }

        public override bool AppliesTo(RequestBinderContext context)
        {
            return context.RequestContext.Route.Parameters.Any() &&
                context.Configuration.HeadersBindingMode != BindingMode.None;
        }

        protected override string MapParameterName(RequestBinderContext context, ActionParameter parameter)
        {
            var bindingMode = context.Configuration.HeadersBindingMode;
            if (bindingMode == BindingMode.Convention && HasHeaderPostfix(parameter.Name))
            {
                return parameter.Name.Truncate(HeaderPostfix.Length);
            }
            if (bindingMode == BindingMode.Explicit || bindingMode == BindingMode.Implicit)
            {
                return parameter.GetAttribute<FromHeadersAttribute>()?.Name ?? parameter.Name;
            }
            return parameter.Name;
        }

        protected override ActionParameter[] GetParameters(RequestBinderContext context)
        {
            var bindingMode = context.Configuration.HeadersBindingMode;
            return context.RequestContext.Route.Parameters
                .Where(x => IncludeParameter(x, bindingMode)).ToArray();
        }

        private bool IncludeParameter(ActionParameter parameter, BindingMode bindingMode)
        {
            return !parameter.HasAttributes<FromUriAttribute, FromBodyAttribute>() &&
                (bindingMode == BindingMode.Implicit ||
                (bindingMode == BindingMode.Convention && HasHeaderPostfix(parameter.Name)) ||
                (bindingMode == BindingMode.Explicit && parameter.HasAttribute<FromHeadersAttribute>()));
        }

        protected override Task<ILookup<string, object>> GetValues(RequestBinderContext context)
        {
            return context.RequestContext.RequestMessage.Headers.ToLookup().ToTaskResult();
        }

        private static bool HasHeaderPostfix(string name)
        {
            return name.Length > HeaderPostfix.Length && name.EndsWith(HeaderPostfix);
        }
    }
}