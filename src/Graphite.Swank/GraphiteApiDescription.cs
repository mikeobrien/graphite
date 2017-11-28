using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Graphite.Actions;
using Swank.Description;

namespace Graphite.Swank
{
    public class GraphiteApiDescription : IApiDescription
    {
        private readonly ActionDescriptor _actionDescriptor;

        public GraphiteApiDescription(ActionDescriptor actionDescriptor)
        {
            _actionDescriptor = actionDescriptor;
            HttpMethod = new HttpMethod(actionDescriptor.Route.Method);
            RequestParameter = new GraphiteApiParameter(actionDescriptor.Route.RequestParameter);
        }
        
        public string Id => _actionDescriptor.Route.Id;
        public string Name => _actionDescriptor.Action.Name;
        public string Documentation { get; } = null;
        public HttpMethod HttpMethod { get; }
        public string RouteTemplate => _actionDescriptor.Route.Url;
        public string RelativePath => _actionDescriptor.Route.Url;
        public MethodInfo ActionMethod => _actionDescriptor.Action.MethodDescriptor.MethodInfo;
        public Type ControllerType => _actionDescriptor.Action.HandlerTypeDescriptor.Type;
        public Type ResponseType => _actionDescriptor.Route.ResponseType.Type;
        public string ResponseDocumentation { get; } = null;
        public IParameterDescription RequestParameter { get; }
        
        public IEnumerable<IParameterDescription> ParameterDescriptions => 
            _actionDescriptor.Route.Parameters
                .Select(x => new GraphiteApiParameter(x.ParameterDescriptor, querystring: true))
                .Union(_actionDescriptor.Route.UrlParameters
                    .Select(x => new GraphiteApiParameter(x.ParameterDescriptor, urlParameter: true)));

        public T GetActionAttribute<T>() where T : Attribute =>
            _actionDescriptor.Action.GetAttribute<T>();

        public T GetControllerAttribute<T>() where T : Attribute =>
            _actionDescriptor.Action.HandlerTypeDescriptor.GetAttribute<T>();

        public bool HasControllerAttribute<T>() where T : Attribute =>
            _actionDescriptor.Action.HandlerTypeDescriptor.HasAttribute<T>();

        public bool HasControllerOrActionAttribute<T>() where T : Attribute =>
            _actionDescriptor.Action.HasActionOrHandlerAttribute<T>();

        public IEnumerable<T> GetControllerAndActionAttributes<T>() where T : Attribute =>
            _actionDescriptor.Action.GetActionOrHandlerAttributes<T>();
    }
}