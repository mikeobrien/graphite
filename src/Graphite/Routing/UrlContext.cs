using System.Collections.Generic;
using Graphite.Actions;
using Graphite.Reflection;

namespace Graphite.Routing
{
    public class UrlContext
    {
        public UrlContext(
            ActionMethod actionMethod, string httpMethod, 
            List<UrlSegment> methodSegments, 
            IEnumerable<UrlParameter> urlParameters,
            IEnumerable<ActionParameter> parameters,
            ParameterDescriptor requestParameter, 
            TypeDescriptor responseType)
        {
            ActionMethod = actionMethod;
            HttpMethod = httpMethod;
            MethodSegments = methodSegments;
            UrlParameters = urlParameters;
            Parameters = parameters;
            RequestParameter = requestParameter;
            ResponseType = responseType;
        }
        
        public virtual ActionMethod ActionMethod { get; }
        public virtual string HttpMethod { get; }
        public virtual List<UrlSegment> MethodSegments { get; }
        public virtual IEnumerable<UrlParameter> UrlParameters { get; }
        public virtual IEnumerable<ActionParameter> Parameters { get; }
        public virtual ParameterDescriptor RequestParameter { get; }
        public virtual TypeDescriptor ResponseType { get; }
    }
}