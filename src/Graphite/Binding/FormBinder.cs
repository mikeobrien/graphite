using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graphite.Extensions;
using Graphite.Http;
using Graphite.Routing;

namespace Graphite.Binding
{
    public class FormBinder : ParameterBinderBase
    {
        public FormBinder(IEnumerable<IValueMapper> mappers) : base(mappers) { }

        public override bool AppliesTo(RequestBinderContext context)
        {
            return !context.RequestContext.Route.HasRequest && 
                context.RequestContext.Route.Parameters.Any() &&
                context.RequestContext.RequestMessage.ContentTypeIs(
                    MimeTypes.ApplicationFormUrlEncoded);
        }

        protected override ActionParameter[] GetParameters(RequestBinderContext context)
        {
            return context.RequestContext.Route.Parameters;
        }

        protected override async Task<ILookup<string, object>> 
            GetValues(RequestBinderContext context)
        {
            var data = await context.RequestContext
                .RequestMessage.Content.ReadAsStringAsync();
            return data.ParseQueryString();
        }
    }
}