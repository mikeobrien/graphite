using System;
using Graphite.Extensions;

namespace Graphite.Reflection
{
    public static class ParseExtensions
    {
        public static bool IsSimpleType(this TypeDescriptor type)
        {
            bool SimpleType(Type x) => x.IsEnum || x == typeof(string) || 
                    x == typeof(decimal) || x == typeof(DateTime) || x == typeof(TimeSpan) || 
                    x == typeof(Guid) || x == typeof(Uri) || x == typeof(bool) || x == typeof(byte) ||
                    x == typeof(sbyte) || x == typeof(ushort) || x == typeof(short) || x == typeof(int) ||
                    x == typeof(uint) || x == typeof(long) || x == typeof(ulong) || x == typeof(char) ||
                    x == typeof(double) || x == typeof(float);

            return SimpleType(type.Type) || (type.IsNullable && SimpleType(type.UnderlyingNullableType.Type));
        }

        public const string EnumParseError = "'{0}' is not valid option.";

        public static ParseResult<T> TryParseEnum<T>(this string value, TypeDescriptor type, Func<T, T> map = null)
        {
            try
            {
                var result = (T)Enum.Parse(type.UnderlyingNullableType.Type, value, true);
                return ParseResult<T>.Succeeded(value, result.TryMap(map));
            }
            catch
            {
                return ParseResult<T>.Failed(value, EnumParseError.ToFormat(value));
            }
        }

        public static ParseResult<T?> TryParseNullableEnum<T>(this string value, 
            TypeDescriptor type, Func<T, T> map = null) where T : struct 
        {
            if (value == null) return ParseResult<T?>.Succeeded(null, null);
            try
            {
                var result = (T?)Enum.Parse(type.UnderlyingNullableType.Type, value, true);
                return ParseResult<T?>.Succeeded(value, ((T)result).TryMap(map));
            }
            catch
            {
                return ParseResult<T?>.Failed(value, EnumParseError.ToFormat(value));
            }
        }

        public const string CharParseError = "Char length {0} is invalid.";

        public static ParseResult<char> TryParseChar(this string value, Func<char, char> map = null)
        {
            return value.Length == 1
                ? ParseResult<char>.Succeeded(value, value[0].TryMap(map))
                : ParseResult<char>.Failed(value, CharParseError.ToFormat(value.Length));
        }
        
        public static ParseResult<char?> TryParseNullableChar(this string value, Func<char, char> map = null)
        {
            if (value == null) return ParseResult<char?>.Succeeded(null, null);
            return value.Length == 1
                ? ParseResult<char?>.Succeeded(value, value[0].TryMap(map))
                : ParseResult<char?>.Failed(value, CharParseError.ToFormat(value.Length));
        }

        public const string BoolParseError = "'{0}' is not a valid boolean. Must be 'true' or 'false'.";

        public static ParseResult<bool> TryParseBool(this string value, Func<bool, bool> map = null)
        {
            bool parsedValue;
            return bool.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<bool>.Succeeded(value, parsedValue.TryMap(map))
                : ParseResult<bool>.Failed(value, BoolParseError.ToFormat(value));
        }
        
        public static ParseResult<bool?> TryParseNullableBool(this string value, Func<bool, bool> map = null)
        {
            if (value == null) return ParseResult<bool?>.Succeeded(null, null);
            bool parsedValue;
            return bool.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<bool?>.Succeeded(value, parsedValue.TryMap(map))
                : ParseResult<bool?>.Failed(value, BoolParseError.ToFormat(value));
        }

        public const string SByteParseError = "'{0}' is not a valid signed byte. Must be an integer between -128 and 127.";

        public static ParseResult<sbyte> TryParseSByte(this string value, Func<sbyte, sbyte> map = null)
        {
            sbyte parsedValue;
            return sbyte.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<sbyte>.Succeeded(value, parsedValue.TryMap(map))
                : ParseResult<sbyte>.Failed(value, SByteParseError.ToFormat(value));
        }
        
        public static ParseResult<sbyte?> TryParseNullableSByte(this string value, Func<sbyte, sbyte> map = null)
        {
            if (value == null) return ParseResult<sbyte?>.Succeeded(null, null);
            sbyte parsedValue;
            return sbyte.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<sbyte?>.Succeeded(value, parsedValue.TryMap(map))
                : ParseResult<sbyte?>.Failed(value, SByteParseError.ToFormat(value));
        }

        public const string ByteParseError = "'{0}' is not a valid byte. Must be an integer between 0 and 255.";

        public static ParseResult<byte> TryParseByte(this string value, Func<byte, byte> map = null)
        {
            byte parsedValue;
            return byte.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<byte>.Succeeded(value, parsedValue.TryMap(map))
                : ParseResult<byte>.Failed(value, ByteParseError.ToFormat(value));
        }
        
        public static ParseResult<byte?> TryParseNullableByte(this string value, Func<byte, byte> map = null)
        {
            if (value == null) return ParseResult<byte?>.Succeeded(null, null);
            byte parsedValue;
            return byte.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<byte?>.Succeeded(value, parsedValue.TryMap(map))
                : ParseResult<byte?>.Failed(value, ByteParseError.ToFormat(value));
        }

        public const string Int16ParseError = "'{0}' is not a valid 16 bit integer. Must be an integer between -32,768 and 32,767.";

        public static ParseResult<short> TryParseInt16(this string value, Func<short, short> map = null)
        {
            short parsedValue;
            return short.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<short>.Succeeded(value, parsedValue.TryMap(map))
                : ParseResult<short>.Failed(value, Int16ParseError.ToFormat(value));
        }
        
        public static ParseResult<short?> TryParseNullableInt16(this string value, Func<short, short> map = null)
        {
            if (value == null) return ParseResult<short?>.Succeeded(null, null);
            short parsedValue;
            return short.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<short?>.Succeeded(value, parsedValue.TryMap(map))
                : ParseResult<short?>.Failed(value, Int16ParseError.ToFormat(value));
        }

        public const string UInt16ParseError = "'{0}' is not a valid 16 bit unsigned integer. Must be an integer between 0 and 65,535.";

        public static ParseResult<ushort> TryParseUInt16(this string value, Func<ushort, ushort> map = null)
        {
            ushort parsedValue;
            return ushort.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<ushort>.Succeeded(value, parsedValue.TryMap(map))
                : ParseResult<ushort>.Failed(value, UInt16ParseError.ToFormat(value));
        }
        
        public static ParseResult<ushort?> TryParseNullableUInt16(this string value, Func<ushort, ushort> map = null)
        {
            if (value == null) return ParseResult<ushort?>.Succeeded(null, null);
            ushort parsedValue;
            return ushort.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<ushort?>.Succeeded(value, parsedValue.TryMap(map))
                : ParseResult<ushort?>.Failed(value, UInt16ParseError.ToFormat(value));
        }

        public const string Int32ParseError = "'{0}' is not a valid 32 bit integer. Must be an integer between -2,147,483,648 and 2,147,483,647.";
 
        public static ParseResult<int> TryParseInt32(this string value, Func<int, int> map = null)
        {
            int parsedValue;
            return int.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<int>.Succeeded(value, parsedValue.TryMap(map))
                : ParseResult<int>.Failed(value, Int32ParseError.ToFormat(value));
        }

        public static ParseResult<int?> TryParseNullableInt32(this string value, Func<int, int> map = null)
        {
            if (value == null) return ParseResult<int?>.Succeeded(null, null);
            int parsedValue;
            return int.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<int?>.Succeeded(value, parsedValue.TryMap(map))
                : ParseResult<int?>.Failed(value, Int32ParseError.ToFormat(value));
        }

        public const string UInt32ParseError = "'{0}' is not a valid 32 bit unsigned integer. Must be an integer between 0 and 4,294,967,295.";

        public static ParseResult<uint> TryParseUInt32(this string value, Func<uint, uint> map = null)
        {
            uint parsedValue;
            return uint.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<uint>.Succeeded(value, parsedValue.TryMap(map))
                : ParseResult<uint>.Failed(value, UInt32ParseError.ToFormat(value));
        }
        
        public static ParseResult<uint?> TryParseNullableUInt32(this string value, Func<uint, uint> map = null)
        {
            if (value == null) return ParseResult<uint?>.Succeeded(null, null);
            uint parsedValue;
            return uint.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<uint?>.Succeeded(value, parsedValue.TryMap(map))
                : ParseResult<uint?>.Failed(value, UInt32ParseError.ToFormat(value));
        }

        public const string Int64ParseError = "'{0}' is not a valid 64 bit integer. Must be an integer between -9,223,372,036,854,775,808 and 9,223,372,036,854,775,807.";

        public static ParseResult<long> TryParseInt64(this string value, Func<long, long> map = null)
        {
            long parsedValue;
            return long.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<long>.Succeeded(value, parsedValue.TryMap(map))
                : ParseResult<long>.Failed(value, Int64ParseError.ToFormat(value));
        }
        
        public static ParseResult<long?> TryParseNullableInt64(this string value, Func<long,long> map = null)
        {
            if (value == null) return ParseResult<long?>.Succeeded(null, null);
            long parsedValue;
            return long.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<long?>.Succeeded(value, parsedValue.TryMap(map))
                : ParseResult<long?>.Failed(value, Int64ParseError.ToFormat(value));
        }

        public const string UInt64ParseError = "'{0}' is not a valid 64 bit unsigned integer. Must be an integer between 0 and 18,446,744,073,709,551,615.";

        public static ParseResult<ulong> TryParseUInt64(this string value, Func<ulong, ulong> map = null)
        {
            ulong parsedValue;
            return ulong.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<ulong>.Succeeded(value, parsedValue.TryMap(map))
                : ParseResult<ulong>.Failed(value, UInt64ParseError.ToFormat(value));
        }
        
        public static ParseResult<ulong?> TryParseNullableUInt64(this string value, Func<ulong, ulong> map = null)
        {
            if (value == null) return ParseResult<ulong?>.Succeeded(null, null);
            ulong parsedValue;
            return ulong.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<ulong?>.Succeeded(value, parsedValue.TryMap(map))
                : ParseResult<ulong?>.Failed(value, UInt64ParseError.ToFormat(value));
        }

        public const string SingleParseError = "'{0}' is not a valid floating point number. Must be a real number between -3.4 x 10^38 and +3.4 x 10^38.";

        public static ParseResult<float> TryParseSingle(this string value, Func<float, float> map = null)
        {
            float parsedValue;
            return float.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<float>.Succeeded(value, parsedValue.TryMap(map))
                : ParseResult<float>.Failed(value, SingleParseError.ToFormat(value));
        }
        
        public static ParseResult<float?> TryParseNullableSingle(this string value, Func<float, float> map = null)
        {
            if (value == null) return ParseResult<float?>.Succeeded(null, null);
            float parsedValue;
            return float.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<float?>.Succeeded(value, parsedValue.TryMap(map))
                : ParseResult<float?>.Failed(value, SingleParseError.ToFormat(value));
        }

        public const string DoubleParseError = "'{0}' is not a valid floating point number. Must be a real number between +/- 5.0 x 10^−324 to +/- 1.7 x 10^308.";

        public static ParseResult<double> TryParseDouble(this string value, Func<double, double> map = null)
        {
            double parsedValue;
            return double.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<double>.Succeeded(value, parsedValue.TryMap(map))
                : ParseResult<double>.Failed(value, DoubleParseError.ToFormat(value));
        }
        
        public static ParseResult<double?> TryParseNullableDouble(this string value, Func<double, double> map = null)
        {
            if (value == null) return ParseResult<double?>.Succeeded(null, null);
            double parsedValue;
            return double.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<double?>.Succeeded(value, parsedValue.TryMap(map))
                : ParseResult<double?>.Failed(value, DoubleParseError.ToFormat(value));
        }

        public const string DecimalParseError = "'{0}' is not a valid floating point number. Must be a real number between +/- 5.0 x 10^−324 to +/- 1.7 x 10^308.";

        public static ParseResult<decimal> TryParseDecimal(this string value, Func<decimal, decimal> map = null)
        {
            decimal parsedValue;
            return decimal.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<decimal>.Succeeded(value, parsedValue.TryMap(map))
                : ParseResult<decimal>.Failed(value, DecimalParseError.ToFormat(value));
        }

        public static ParseResult<decimal?> TryParseNullableDecimal(this string value, Func<decimal, decimal> map = null)
        {
            if (value == null) return ParseResult<decimal?>.Succeeded(null, null);
            decimal parsedValue;
            return decimal.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<decimal?>.Succeeded(value, parsedValue.TryMap(map))
                : ParseResult<decimal?>.Failed(value, DecimalParseError.ToFormat(value));
        }

        public const string DateTimeParseError = "'{0}' is not a valid datetime.";

        public static ParseResult<DateTime> TryParseDateTime(this string value, Func<DateTime, DateTime> map = null)
        {
            DateTime parsedValue;
            return DateTime.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<DateTime>.Succeeded(value, parsedValue.TryMap(map))
                : ParseResult<DateTime>.Failed(value, DateTimeParseError.ToFormat(value));
        }

        public static ParseResult<DateTime?> TryParseNullableDateTime(this string value, Func<DateTime, DateTime> map = null)
        {
            if (value == null) return ParseResult<DateTime?>.Succeeded(null, null);
            DateTime parsedValue;
            return DateTime.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<DateTime?>.Succeeded(value, parsedValue.TryMap(map))
                : ParseResult<DateTime?>.Failed(value, DateTimeParseError.ToFormat(value));
        }

        public const string GuidParseError = "'{0}' is not a valid uuid. Should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).";

        public static ParseResult<Guid> TryParseGuid(this string value, Func<Guid, Guid> map = null)
        {
            Guid parsedValue;
            return Guid.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<Guid>.Succeeded(value, parsedValue.TryMap(map))
                : ParseResult<Guid>.Failed(value, GuidParseError.ToFormat(value));
        }
        
        public static ParseResult<Guid?> TryParseNullableGuid(this string value, Func<Guid, Guid> map = null)
        {
            if (value == null) return ParseResult<Guid?>.Succeeded(null, null);
            Guid parsedValue;
            return Guid.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<Guid?>.Succeeded(value, parsedValue.TryMap(map))
                : ParseResult<Guid?>.Failed(value, GuidParseError.ToFormat(value));
        }

        public const string TimeSpanParseError = "'{0}' is not a valid time span. Must be in the format 'h:m:s'.";

        public static ParseResult<TimeSpan> TryParseTimeSpan(this string value, Func<TimeSpan, TimeSpan> map = null)
        {
            TimeSpan parsedValue;
            return TimeSpan.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<TimeSpan>.Succeeded(value, parsedValue.TryMap(map))
                : ParseResult<TimeSpan>.Failed(value, TimeSpanParseError.ToFormat(value));
        }

        public static ParseResult<TimeSpan?> TryParseNullableTimeSpan(this string value, Func<TimeSpan, TimeSpan> map = null)
        {
            if (value == null) return ParseResult<TimeSpan?>.Succeeded(null, null);
            TimeSpan parsedValue;
            return TimeSpan.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<TimeSpan?>.Succeeded(value, parsedValue.TryMap(map))
                : ParseResult<TimeSpan?>.Failed(value, TimeSpanParseError.ToFormat(value));
        }
        
        public const string UriParseError = "'{0}' is not valid url.";

        public static ParseResult<Uri> TryParseUri(this string value, Func<Uri, Uri> map = null)
        {
            try
            {
                return ParseResult<Uri>.Succeeded(value, new Uri(value).TryMap(map));
            }
            catch
            {
                return ParseResult<Uri>.Failed(value, UriParseError.ToFormat(value));
            }
        }

        private static T TryMap<T>(this T value, Func<T, T> map)
        {
            return map != null ? map(value) : value;
        }
    }
}
