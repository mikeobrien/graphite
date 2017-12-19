using System;
using Graphite.Extensions;
using Graphite.Reflection;
using EnumMapper = System.Func<Graphite.Binding.SimpleTypeMapper, 
    Graphite.Binding.ValueMapperContext, Graphite.Binding.MapResult>;

namespace Graphite.Binding
{
    public class SimpleTypeMapper : IValueMapper
    {
        private static readonly Func<Type, EnumMapper> EnumMappers = Memoize.Func<Type, EnumMapper>(x => x
            .CloseGenericMethod<SimpleTypeMapper, ValueMapperContext, MapResult>(i => i.MapEnum<int>(null)));
        private static readonly Func<Type, EnumMapper> NullableEnumMappers = Memoize.Func<Type, EnumMapper>(x => x
            .CloseGenericMethod<SimpleTypeMapper, ValueMapperContext, MapResult>(i => i.MapNullableEnum<int>(null)));

        private readonly ParsedValueMapper _mapper;
        
        public SimpleTypeMapper(ParsedValueMapper mapper)
        {
            _mapper = mapper;
        }

        public bool AppliesTo(ValueMapperContext context)
        {
            return (context.Type.ElementType ?? context.Type).IsSimpleType;
        }

        public MapResult Map(ValueMapperContext context)
        {
            var type = context.Type.ElementType ?? context.Type;
            
            if (!type.IsNullable && type.Type.IsEnum) return EnumMappers(type.Type)(this, context);
            if (type.IsNullable && type.UnderlyingNullableType.Type.IsEnum)
                return NullableEnumMappers(type.UnderlyingNullableType.Type)(this, context);
            if (type.Type.Is<string>()) return _mapper.Map(context, x => ParseResult<string>.Succeeded(x, x));
            if (type.Type.Is<char>()) return _mapper.Map(context, x => x.TryParseChar());
            if (type.Type.Is<char?>()) return _mapper.Map(context, x => x.TryParseNullableChar());
            if (type.Type.Is<bool>()) return _mapper.Map(context, x => x.TryParseBool());
            if (type.Type.Is<bool?>()) return _mapper.Map(context, x => x.TryParseNullableBool());
            if (type.Type.Is<sbyte>()) return _mapper.Map(context, x => x.TryParseSByte());
            if (type.Type.Is<sbyte?>()) return _mapper.Map(context, x => x.TryParseNullableSByte());
            if (type.Type.Is<byte>()) return _mapper.Map(context, x => x.TryParseByte());
            if (type.Type.Is<byte?>()) return _mapper.Map(context, x => x.TryParseNullableByte());
            if (type.Type.Is<short>()) return _mapper.Map(context, x => x.TryParseInt16());
            if (type.Type.Is<short?>()) return _mapper.Map(context, x => x.TryParseNullableInt16());
            if (type.Type.Is<ushort>()) return _mapper.Map(context, x => x.TryParseUInt16());
            if (type.Type.Is<ushort?>()) return _mapper.Map(context, x => x.TryParseNullableUInt16());
            if (type.Type.Is<int>()) return _mapper.Map(context, x => x.TryParseInt32());
            if (type.Type.Is<int?>()) return _mapper.Map(context, x => x.TryParseNullableInt32());
            if (type.Type.Is<uint>()) return _mapper.Map(context, x => x.TryParseUInt32());
            if (type.Type.Is<uint?>()) return _mapper.Map(context, x => x.TryParseNullableUInt32());
            if (type.Type.Is<long>()) return _mapper.Map(context, x => x.TryParseInt64());
            if (type.Type.Is<long?>()) return _mapper.Map(context, x => x.TryParseNullableInt64());
            if (type.Type.Is<ulong>()) return _mapper.Map(context, x => x.TryParseUInt64());
            if (type.Type.Is<ulong?>()) return _mapper.Map(context, x => x.TryParseNullableUInt64());
            if (type.Type.Is<float>()) return _mapper.Map(context, x => x.TryParseSingle());
            if (type.Type.Is<float?>()) return _mapper.Map(context, x => x.TryParseNullableSingle());
            if (type.Type.Is<double>()) return _mapper.Map(context, x => x.TryParseDouble());
            if (type.Type.Is<double?>()) return _mapper.Map(context, x => x.TryParseNullableDouble());
            if (type.Type.Is<decimal>()) return _mapper.Map(context, x => x.TryParseDecimal());
            if (type.Type.Is<decimal?>()) return _mapper.Map(context, x => x.TryParseNullableDecimal());
            if (type.Type.Is<DateTime>()) return _mapper.Map(context, x => x.TryParseDateTime());
            if (type.Type.Is<DateTime?>()) return _mapper.Map(context, x => x.TryParseNullableDateTime());
            if (type.Type.Is<Guid>()) return _mapper.Map(context, x => x.TryParseGuid());
            if (type.Type.Is<Guid?>()) return _mapper.Map(context, x => x.TryParseNullableGuid());
            if (type.Type.Is<TimeSpan>()) return _mapper.Map(context, x => x.TryParseTimeSpan());
            if (type.Type.Is<TimeSpan?>()) return _mapper.Map(context, x => x.TryParseNullableTimeSpan());
            if (type.Type.Is<Uri>()) return _mapper.Map(context, x => x.TryParseUri());
            return MapResult.NoMapper();
        }

        private MapResult MapEnum<T>(ValueMapperContext context)
        {
            return _mapper.Map(context, x => x.TryParseEnum<T>(
                context.Type.ElementType ?? context.Type));
        }

        private MapResult MapNullableEnum<T>(ValueMapperContext context) where T : struct 
        {
            return _mapper.Map(context, x => x.TryParseNullableEnum<T>(
                context.Type.ElementType ?? context.Type));
        }
    }
}
