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
            string[] urlSegments, ParameterDescriptor[] urlParameters,
            ParameterDescriptor[] wildcardParameters,
            ParameterDescriptor[] querystringParameters,
            ParameterDescriptor requestParameter, 
            TypeDescriptor responseBodyType)
        {
            Configuration = configuration;
            HttpConfiguration = httpConfiguration;
            ActionMethod = actionMethod;
            HttpMethod = httpMethod;
            UrlSegments = urlSegments;
            UrlParameters = urlParameters;
            WildcardParameters = wildcardParameters;
            QuerystringParameters = querystringParameters;
            RequestParameter = requestParameter;
            ResponseBodyType = responseBodyType;
        }

        public virtual Configuration Configuration { get; }
        public virtual HttpConfiguration HttpConfiguration { get; }
        public virtual ActionMethod ActionMethod { get; }
        public virtual string HttpMethod { get; }
        public virtual string[] UrlSegments { get; }
        public virtual ParameterDescriptor[] UrlParameters { get; }
        public virtual ParameterDescriptor[] WildcardParameters { get; }
        public virtual ParameterDescriptor[] QuerystringParameters { get; }
        public virtual ParameterDescriptor RequestParameter { get; }
        public virtual TypeDescriptor ResponseBodyType { get; }
    }

    public interface IUrlConvention : IConditional<UrlContext>
    {
        string[] GetUrls(UrlContext context);
    }
}
