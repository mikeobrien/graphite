using System.Collections.Generic;
using System.Linq;
using Graphite.Reflection;

namespace Graphite.Actions
{
    public class DefaultActionMethodSource : IActionMethodSource
    {
        private readonly Configuration _configuration;
        private readonly ITypeCache _typeCache;

        public DefaultActionMethodSource(Configuration configuration, ITypeCache typeCache)
        {
            _configuration = configuration;
            _typeCache = typeCache;
        }

        public virtual bool Applies()
        {
            return true;
        }

        public virtual IEnumerable<ActionMethod> GetActionMethods()
        {
            return _configuration.Assemblies
                .SelectMany(x => _typeCache.GetTypeDescriptors(x))
                .Where(x => 
                    _configuration.HandlerNameConvention.IsMatch(x.Name) && 
                    _configuration.HandlerFilter(_configuration, x))
                .SelectMany(t => t.Methods
                    .Where(m => !m.MethodInfo.IsGenericMethodDefinition && !m.IsBclMethod &&
                        _configuration.ActionNameConvention(_configuration).IsMatch(m.Name) && 
                        _configuration.ActionFilter(_configuration, m))
                    .Select(m => new ActionMethod(t, m)));
        }
    }
}
