using System;
using System.Linq;
using System.Net.Http;
using Graphite.Actions;
using Graphite.Binding;
using Graphite.Extensions;
using Graphite.Http;
using Graphite.Routing;

namespace Graphite.Readers
{
    public class FormReader : StringReaderBase
    {
        private readonly ParameterBinder<ReadResult> _parameterBinder;
        private readonly ActionMethod _actionMethod;
        private readonly RouteDescriptor _routeDescriptor;

        public FormReader(
            ParameterBinder<ReadResult> parameterBinder,
            ActionMethod actionMethod, RouteDescriptor routeDescriptor, 
            HttpRequestMessage requestMessage) 
            : base(requestMessage, MimeTypes.ApplicationFormUrlEncoded)
        {
            _parameterBinder = parameterBinder;
            _actionMethod = actionMethod;
            _routeDescriptor = routeDescriptor;
        }

        protected override ReadResult GetRequest(string data)
        {
            var requestParameter = _routeDescriptor.RequestParameter;
            var parameterType = requestParameter.ParameterType;
            var instance = parameterType.TryCreate();

            if (instance == null)
                throw new RequestParameterCreationException(
                    requestParameter, _actionMethod);

            var actionParameters = parameterType.Properties.Select(x =>
                new ActionParameter(_actionMethod, requestParameter, x));
            var values = data.ParseQueryString();

            return _parameterBinder.Bind(values, actionParameters,
                (p, v) => p.BindProperty(instance, v),
                () => ReadResult.Success(instance),
                ReadResult.Failure);
        }
    }
}