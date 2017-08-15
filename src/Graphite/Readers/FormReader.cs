using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Graphite.Actions;
using Graphite.Binding;
using Graphite.Extensions;
using Graphite.Http;
using Graphite.Linq;
using Graphite.Routing;

namespace Graphite.Readers
{
    public class FormReader : StringReaderBase
    {
        private readonly Configuration _configuration;
        private readonly HttpConfiguration _httpConfiguration;
        private readonly IEnumerable<IValueMapper> _mappers;
        private readonly ActionMethod _actionMethod;
        private readonly RouteDescriptor _routeDescriptor;

        public FormReader(Configuration configuration, HttpConfiguration httpConfiguration,
            ActionMethod actionMethod, RouteDescriptor routeDescriptor, 
            IEnumerable<IValueMapper> mappers, HttpRequestMessage requestMessage) 
            : base(requestMessage, MimeTypes.ApplicationFormUrlEncoded)
        {
            _configuration = configuration;
            _httpConfiguration = httpConfiguration;
            _mappers = mappers;
            _actionMethod = actionMethod;
            _routeDescriptor = routeDescriptor;
        }

        protected override object GetRequest(string data)
        {
            var requestParameter = _routeDescriptor.RequestParameter;
            var parameterType = requestParameter.ParameterType;
            var instance = parameterType.TryCreate();

            if (instance == null) return null;

            data.ParseQueryString()
                .Where(x => x.Any())
                .JoinIgnoreCase(parameterType.Properties, x => x.Key, x => x.Name, 
                    (param, prop) => new
                    {
                        Parameter = new ActionParameter(requestParameter, prop),
                        Values = param.ToArray()
                    })
                .ForEach(x =>
                {
                    var result = _mappers.Map(_actionMethod, _routeDescriptor, 
                        x.Parameter, x.Values, _configuration, _httpConfiguration);
                    if (result.Mapped) x.Parameter.BindProperty(instance, result.Value);
                });

            return instance;
        }
    }
}