using System.Web.Http;
using Graphite.Actions;
using Graphite.Extensibility;
using Graphite.Reflection;

namespace Graphite.Routing
{
    public class UrlContext
    {
        public UrlContext(Configuration configuration, 
            HttpConfiguration httpConfiguration,
            ActionMethod actionMethod, string httpMethod, 
            string[] urlSegments, UrlParameter[] urlParameters,
            ActionParameter[] parameters,
            ParameterDescriptor requestParameter, 
            TypeDescriptor responseType)
        {
            Configuration = configuration;
            HttpConfiguration = httpConfiguration;
            ActionMethod = actionMethod;
            HttpMethod = httpMethod;
            UrlSegments = urlSegments;
            UrlParameters = urlParameters;
            Parameters = parameters;
            RequestParameter = requestParameter;
            ResponseType = responseType;
        }

        public virtual Configuration Configuration { get; }
        public virtual HttpConfiguration HttpConfiguration { get; }
        public virtual ActionMethod ActionMethod { get; }
        public virtual string HttpMethod { get; }
        public virtual string[] UrlSegments { get; }
        public virtual UrlParameter[] UrlParameters { get; }
        public virtual ActionParameter[] Parameters { get; }
        public virtual ParameterDescriptor RequestParameter { get; }
        public virtual TypeDescriptor ResponseType { get; }
    }

    public interface IUrlConvention : IConditional<UrlContext>
    {
        string[] GetUrls(UrlContext context);
    }
}
