using Graphite.Authentication;
using Graphite.Behaviors;
using Graphite.Binding;
using Graphite.DependencyInjection;
using Graphite.Extensibility;
using Graphite.Routing;
using Graphite.Extensions;
using Graphite.Readers;
using Graphite.Reflection;
using Graphite.Writers;

namespace Graphite.Actions
{
    public class ActionDescriptor
    {
        public ActionDescriptor(
            ActionMethod action,
            RouteDescriptor route, 
            Plugins<IAuthenticator> authenticators, 
            Plugins<IRequestBinder> requestBinders, 
            Plugins<IRequestReader> requestReaders, 
            Plugins<IResponseWriter> responseWriters,
            Plugins<IResponseStatus> responseStatus,
            Plugins<IResponseHeaders> responseHeaders,
            Plugins<IBehavior> behaviors,
            ITypeCache typeCache)
        {
            Action = action;
            Route = route;
            Authenticators = authenticators;
            RequestBinders = requestBinders;
            RequestReaders = requestReaders;
            ResponseWriters = responseWriters;
            ResponseStatus = responseStatus;
            ResponseHeaders = responseHeaders;
            Behaviors = behaviors;
            Registry = new Registry(typeCache);
        }
        
        public virtual ActionMethod Action { get; }
        public virtual RouteDescriptor Route { get; }
        public virtual Registry Registry { get; }
        
        public virtual Plugins<IAuthenticator> Authenticators { get; }
        public virtual Plugins<IRequestBinder> RequestBinders { get; }
        public virtual Plugins<IRequestReader> RequestReaders { get; } 
        public virtual Plugins<IResponseWriter> ResponseWriters { get; }
        public virtual Plugins<IResponseStatus> ResponseStatus { get; }
        public virtual Plugins<IResponseHeaders> ResponseHeaders { get; }
        public virtual Plugins<IBehavior> Behaviors { get; }

        public override int GetHashCode()
        {
            return this.GetHashCode(Route?.Id);
        }

        public override bool Equals(object obj)
        {
            return GetHashCode() == obj?.GetHashCode();
        }

        public override string ToString()
        {
            return Route.Id;
        }
    }
}