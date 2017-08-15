using System.Collections.Generic;
using System.Web.Http;
using Graphite.Actions;
using Graphite.Reflection;

namespace Graphite.Routing
{
    public class UrlConfigurationContext
    {
        public UrlConfigurationContext(Configuration configuration, 
            HttpConfiguration httpConfiguration, UrlContext urlContext)
        {
            Configuration = configuration;
            HttpConfiguration = httpConfiguration;
            ActionMethod = urlContext.ActionMethod;
            HttpMethod = urlContext.HttpMethod;
            Url = urlContext.Url;
            UrlParameters = urlContext.UrlParameters;
            Parameters = urlContext.Parameters;
            RequestParameter = urlContext.RequestParameter;
            ResponseType = urlContext.ResponseType;
        }

        public Configuration Configuration { get; }
        public HttpConfiguration HttpConfiguration { get; }
        public virtual ActionMethod ActionMethod { get; }
        public virtual string HttpMethod { get; }
        public virtual Url Url { get; }
        public virtual IEnumerable<UrlParameter> UrlParameters { get; }
        public virtual IEnumerable<ActionParameter> Parameters { get; }
        public virtual ParameterDescriptor RequestParameter { get; }
        public virtual TypeDescriptor ResponseType { get; }
    }
}