using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graphite.Extensions;
using Graphite.Routing;

namespace Graphite.Binding
{
    public abstract class ParameterBinderBase : IRequestBinder
    {
        private readonly IEnumerable<IValueMapper> _mappers;

        protected ParameterBinderBase(IEnumerable<IValueMapper> mappers)
        {
            _mappers = mappers;
        }

        public abstract bool AppliesTo(RequestBinderContext context);

        protected abstract Task<ILookup<string, object>> GetValues(RequestBinderContext context);
        protected abstract ActionParameter[] GetParameters(RequestBinderContext context);

        protected virtual string MapParameterName(RequestBinderContext context, ActionParameter parameter)
        {
            return parameter.Name;
        }

        public virtual async Task Bind(RequestBinderContext context)
        {
            var values = await GetValues(context);
            var actionParameters = GetParameters(context)
                .Where(x => x.IsParameter || x.IsPropertyOfParameter);

            values.Where(x => x.Any())
                .JoinIgnoreCase(actionParameters, x => x.Key, 
                    x => MapParameterName(context, x), 
                    (p, ap) => new
                    {
                        ActionParameter = ap,
                        Values = p.ToArray()
                    })
                .ForEach(x =>
                {
                    var result = _mappers.Map(x.Values, x.ActionParameter,
                        context.RequestContext, context.Configuration);

                    if (!result.Mapped) return;

                    x.ActionParameter.BindArgument(context.ActionArguments, result.Value);
                });
        }
    }
}