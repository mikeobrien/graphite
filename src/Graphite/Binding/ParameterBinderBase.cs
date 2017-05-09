using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graphite.Reflection;

namespace Graphite.Binding
{
    public abstract class ParameterBinderBase : IRequestBinder
    {
        private readonly IEnumerable<IValueMapper> _mappers;
        private readonly Configuration _configuration;

        protected ParameterBinderBase(IEnumerable<IValueMapper> mappers,
            Configuration configuration)
        {
            _mappers = mappers;
            _configuration = configuration;
        }

        public abstract bool AppliesTo(RequestBinderContext context);
        protected abstract ParameterDescriptor[] GetParameters(RequestBinderContext context);
        protected abstract Task<ILookup<string, string>> GetValues(RequestBinderContext context);

        public virtual async Task Bind(RequestBinderContext context)
        {
            var parameters = await GetValues(context);
            foreach (var parameter in GetParameters(context))
            {
                var values = parameters[parameter.Name].ToArray();
                var result = _mappers.Map(values, parameter, context.RequestContext, _configuration);
                if (result.Mapped) context.ActionArguments[parameter.Position] = result.Value;
            }
        }
    }
}