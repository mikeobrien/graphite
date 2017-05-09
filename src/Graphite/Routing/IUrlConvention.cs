using Graphite.Actions;
using Graphite.Extensibility;
using Graphite.Reflection;

namespace Graphite.Routing
{
    public class UrlContext
    {
        public UrlContext(
            ActionMethod actionMethod, string httpMethod, 
            Url url, UrlParameter[] urlParameters,
            ActionParameter[] parameters,
            ParameterDescriptor requestParameter, 
            TypeDescriptor responseType)
        {
            ActionMethod = actionMethod;
            HttpMethod = httpMethod;
            Url = url;
            UrlParameters = urlParameters;
            Parameters = parameters;
            RequestParameter = requestParameter;
            ResponseType = responseType;
        }
        
        public virtual ActionMethod ActionMethod { get; }
        public virtual string HttpMethod { get; }
        public virtual Url Url { get; }
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
