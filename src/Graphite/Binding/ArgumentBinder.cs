using System;
using System.Collections.Generic;
using System.Linq;
using Graphite.Routing;

namespace Graphite.Binding
{
    public class ArgumentBinder
    {
        private readonly ParameterBinder<BindResult> _parameterBinder;

        public ArgumentBinder(ParameterBinder<BindResult> parameterBinder)
        {
            _parameterBinder = parameterBinder;
        }

        public virtual BindResult Bind(ILookup<string, object> values, object[] actionArguments,
            IEnumerable<ActionParameter> parameters, Func<ActionParameter, string> mapName = null)
        {
            if (values == null || !values.Any()) return BindResult.Success();

            var actionParameters = parameters.Where(x => x.IsParameter || x.IsPropertyOfParameter);

            return _parameterBinder.Bind(values, actionParameters,
                (p, v) => p.BindArgument(actionArguments, v),
                BindResult.Success, BindResult.Failure, mapName);
        }
    }
}