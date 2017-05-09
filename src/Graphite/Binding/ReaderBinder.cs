using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Graphite.Readers;

namespace Graphite.Binding
{
    public class ReaderBinder : IRequestBinder
    {
        private readonly IEnumerable<IRequestReader> _readers;
        private readonly Configuration _configuration;

        public ReaderBinder(IEnumerable<IRequestReader> readers,
            Configuration configuration)
        {
            _readers = readers;
            _configuration = configuration;
        }

        public virtual bool AppliesTo(RequestBinderContext context)
        {
            return context.RequestContext.Route.HasRequest;
        }

        public virtual async Task Bind(RequestBinderContext context)
        {
            var route = context.RequestContext.Route;
            var position = route.RequestParameter.Position;
            var reader = _readers.ThatApplyTo(context.RequestContext, _configuration);
            if (reader != null) context.ActionArguments[position] = await reader.Read(context);
        }
    }
}