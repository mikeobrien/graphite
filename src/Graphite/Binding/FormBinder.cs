using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graphite.Extensions;
using Graphite.Http;
using Graphite.Reflection;

namespace Graphite.Binding
{
    public class FormBinder : ParameterBinderBase
    {
        public FormBinder(IEnumerable<IValueMapper> mappers,
            Configuration configuration) : base(mappers, configuration) { }

        public override bool AppliesTo(RequestBinderContext context)
        {
            return !context.RequestContext.Route.HasRequest && 
                context.RequestContext.Route.QuerystringParameters.Any() &&
                context.RequestContext.RequestMessage.ContentTypeIs(
                    MimeTypes.ApplicationFormUrlEncoded);
        }

        protected override ParameterDescriptor[] 
            GetParameters(RequestBinderContext context)
        {
            return context.RequestContext.Route.QuerystringParameters;
        }

        protected override async Task<ILookup<string, string>> 
            GetValues(RequestBinderContext context)
        {
            var data = await context.RequestContext
                .RequestMessage.Content.ReadAsStringAsync();
            return data.ParseQueryString();
        }
    }
}