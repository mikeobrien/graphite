using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using Graphite.Actions;
using Graphite.Extensions;
using Graphite.Http;
using Graphite.Routing;
using RequestContext = Graphite.Actions.RequestContext;

namespace Graphite.Binding
{
    public class FromRequestInfoAttribute : Attribute
    {
        public FromRequestInfoAttribute() { }

        public FromRequestInfoAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    public class RequestInfoBinder : ParameterBinderBase
    {
        public const string HttpContextKey = "MS_HttpContext";

        private static readonly Dictionary<Type, Func<RequestContext, object>> TypeMaps =
            new Dictionary<Type, Func<RequestContext, object>>
            {
                { typeof(ActionMethod), x => x.Action },
                { typeof(RouteDescriptor), x => x.Route },
                { typeof(UrlParameters), x => x.UrlParameters },
                { typeof(QuerystringParameters), x => x.QuerystringParameters },
                { typeof(HttpRequestMessage), x => x.RequestMessage },
                { typeof(HttpConfiguration), x => x.HttpConfiguration },
                { typeof(HttpRequestContext), x => x.HttpRequestContext }
            };

        public RequestInfoBinder(IEnumerable<IValueMapper> mappers) : base(mappers) { }

        public override bool AppliesTo(RequestBinderContext context)
        {
            return context.RequestContext.Route.Parameters.Any() &&
                context.Configuration.RequestInfoBindingMode != BindingMode.None;
        }

        public override Task Bind(RequestBinderContext context)
        {
            BindByType(context);
            return base.Bind(context);
        }

        protected override string MapParameterName(RequestBinderContext context, ActionParameter parameter)
        {
            return parameter.GetAttribute<FromRequestInfoAttribute>()?.Name ?? parameter.Name;
        }

        protected override ActionParameter[] GetParameters(RequestBinderContext context)
        {
            var bindingMode = context.Configuration.RequestInfoBindingMode;
            return context.RequestContext.Route.Parameters
                .Where(x => IncludeParameter(x, bindingMode)).ToArray();
        }

        private void BindByType(RequestBinderContext context)
        {
            var bindingMode = context.Configuration.RequestInfoBindingMode;
            context.RequestContext.Route.Parameters
                .Where(x => IncludeParameter(x, bindingMode))
                .Join(TypeMaps, x => x.TypeDescriptor.Type, x => x.Key, 
                    (p, m) => new { Parameter = p, Map = m.Value })
                .ForEach(x => x.Parameter.BindArgument(context.ActionArguments, 
                    x.Map(context.RequestContext)));
        }

        private bool IncludeParameter(ActionParameter parameter, BindingMode bindingMode)
        {
            return !parameter.HasAttributes<FromUriAttribute, FromBodyAttribute>() &&
                (bindingMode == BindingMode.Implicit ||
                (bindingMode == BindingMode.Explicit && parameter.HasAttribute<FromRequestInfoAttribute>()));
        }

        protected override Task<ILookup<string, object>> GetValues(RequestBinderContext context)
        {
            var request = context.RequestContext.RequestMessage;

            if (request.Properties.ContainsKey(HttpContextKey))
            {
                return request.Properties[HttpContextKey].As<HttpContextBase>()?.Request?
                    .ServerVariables.ToLookup(NormalizeServerVariableKey).ToTaskResult();
            }
            if (request.Properties.ContainsKey(RemoteEndpointMessageProperty.Name))
            {
                var endpoint = (RemoteEndpointMessageProperty)request
                   .Properties[RemoteEndpointMessageProperty.Name];
                return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    { "RemoteAddress", endpoint.Address },
                    { "RemotePort", endpoint.Port }
                }.ToLookup().ToTaskResult();
            }
            return ((ILookup<string, object>)null).ToTaskResult();
        }

        private static string NormalizeServerVariableKey(string key)
        {
            return key.Replace("_ADDR", "address").Replace("_", "");
        }
    }
}