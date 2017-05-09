using Graphite.Reflection;

namespace Graphite.Routing
{
    public class UrlParameter : ActionParameter
    {
        public UrlParameter(ParameterDescriptor parameter, 
            bool isWildcard) : base(parameter)
        {
            IsWildcard = isWildcard;
        }

        public UrlParameter(ActionParameter parameter,
            bool isWildcard) : base(parameter)
        {
            IsWildcard = isWildcard;
        }

        public bool IsWildcard { get; }
    }
}