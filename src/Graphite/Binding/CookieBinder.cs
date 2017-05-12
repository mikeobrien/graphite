using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Graphite.Extensions;
using Graphite.Routing;

namespace Graphite.Binding
{
    public class FromCookiesAttribute : Attribute
    {
        public FromCookiesAttribute() { }

        public FromCookiesAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    public class CookieBinder : ParameterBinderBase
    {
        public const string CookiePostfix = "Cookie";

        public CookieBinder(IEnumerable<IValueMapper> mappers) : base(mappers) { }

        public override bool AppliesTo(RequestBinderContext context)
        {
            return context.RequestContext.Route.Parameters.Any() &&
                context.Configuration.CookiesBindingMode != BindingMode.None;
        }

        protected override string MapParameterName(RequestBinderContext context, ActionParameter parameter)
        {
            var bindingMode = context.Configuration.CookiesBindingMode;
            if (bindingMode == BindingMode.Convention && HasCookiePostfix(parameter.Name))
            {
                return parameter.Name.Truncate(CookiePostfix.Length);
            }
            if (bindingMode == BindingMode.Explicit || bindingMode == BindingMode.Implicit)
            {
                return parameter.GetAttribute<FromCookiesAttribute>()?.Name ?? parameter.Name;
            }
            return parameter.Name;
        }

        protected override ActionParameter[] GetParameters(RequestBinderContext context)
        {
            var bindingMode = context.Configuration.CookiesBindingMode;
            return context.RequestContext.Route.Parameters
                .Where(x => IncludeParameter(x, bindingMode)).ToArray();
        }

        private bool IncludeParameter(ActionParameter parameter, BindingMode bindingMode)
        {
            return !parameter.HasAttributes<FromUriAttribute, FromBodyAttribute>() &&
                (bindingMode == BindingMode.Implicit ||
                (bindingMode == BindingMode.Convention && HasCookiePostfix(parameter.Name)) ||
                (bindingMode == BindingMode.Explicit && parameter.HasAttribute<FromCookiesAttribute>()));
        }

        protected override Task<ILookup<string, object>> GetValues(RequestBinderContext context)
        {
            return context.RequestContext.RequestMessage
                .Headers.GetCookies().ToLookup().ToTaskResult();
        }

        private static bool HasCookiePostfix(string name)
        {
            return name.Length > CookiePostfix.Length && name.EndsWith(CookiePostfix);
        }
    }
}