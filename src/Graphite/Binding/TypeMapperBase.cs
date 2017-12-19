using Graphite.Reflection;

namespace Graphite.Binding
{
    public abstract class TypeMapperBase<T> : IValueMapper
    {
        private readonly ParsedValueMapper _mapper;
        
        protected TypeMapperBase(ParsedValueMapper mapper)
        {
            _mapper = mapper;
        }

        public bool AppliesTo(ValueMapperContext context)
        {
            return (context.Type.ElementType ?? context.Type).Type == typeof(T);
        }

        protected abstract ParseResult<T> Parse(string value);

        public MapResult Map(ValueMapperContext context)
        {
            return _mapper.Map(context, Parse);
        }
    }
}