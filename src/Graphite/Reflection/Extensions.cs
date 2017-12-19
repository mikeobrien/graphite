using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Graphite.Extensions;
using Graphite.Linq;

namespace Graphite.Reflection
{
    public static class ReflectionExtensions
    {
        // This is a nasty peice of work but the only way
        // to get around the ReflectionTypeLoadException
        // in certian situations so it is what it is.
        public static Type[] GetTypesSafely(this Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                return exception.Types.Where(x => x != null).ToArray();
            }
        }

        public static TypeDescriptor GetTypeDescriptor<T>(this ITypeCache typeCache)
        {
            return typeCache.GetTypeDescriptor(typeof(T));
        }

        public static Func<object> CompileTryCreate(this Type type)
        {
            var constructor = type.GetParameterlessConstructor();
            return Expression.Lambda<Func<object>>(constructor == null ?
                (Expression)Expression.Constant(null) :
                Expression.New(constructor)).Compile();
        }

        public static Func<object, object> CompileGetter(this PropertyInfo property)
        {
            var instance = Expression.Parameter(typeof(object));
            return Expression.Lambda<Func<object, object>>(instance
                    .Convert(property.DeclaringType)
                    .PropertyAccess(property)
                    .Convert<object>(),
                instance).Compile();
        }

        public static Action<object, object> CompileSetter(this PropertyInfo property)
        {
            var instance = Expression.Parameter(typeof(object));
            var value = Expression.Parameter(typeof(object));
            return Expression.Lambda<Action<object, object>>(instance
                    .Convert(property.DeclaringType)
                    .PropertyAccess(property).Assign(value.Convert(property.PropertyType)),
                instance, value).Compile();
        }

        public static bool IsInDebugMode(this Assembly assembly)
        {
            return assembly.GetCustomAttributes(typeof(DebuggableAttribute), false)
                .Cast<DebuggableAttribute>()
                .Any(x => x.IsJITTrackingEnabled && x.IsJITOptimizerDisabled);
        }

        public static string GetNestedName(this Type type)
        {
            return type.Enumerate(x => x.DeclaringType)
                .Reverse().Select(x => x.Name).Join('+');
        }

        public static string GetNonGenericName(this Type type)
        {
            return GetNonGenericName(type.Name);
        }

        public static string GetNonGenericFullName(this Type type)
        {
            return GetNonGenericName(type.FullName);
        }

        public static string GetNonGenericName(this string name)
        {
            var index = name.IndexOf('`');
            return index > 0 ? name.Remove(index) : name;
        }

        public static string NormalizeNestedTypeName(this string typeName)
        {
            return typeName.Replace('+', '.');
        }

        private static readonly Dictionary<Type, string> 
            CSharpBuiltInTypeName = new Dictionary<Type, string>
        {
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(sbyte), "sbyte" },
            { typeof(char), "char" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(float), "float" },
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(string), "string" },
            { typeof(object), "object" },
            { typeof(void), "void" }
        };

        public static string GetFriendlyTypeName(this Type type, bool includeNamespace = false)
        {
            var underlyingType = type.GetUnderlyingNullableType();
            var @namespace = includeNamespace ? $"{underlyingType.Namespace}." : "";
            if (includeNamespace && underlyingType.DeclaringType != null) 
                @namespace += $"{underlyingType.DeclaringType.Name}.";
            return CSharpBuiltInTypeName.ContainsKey(type) 
                ? CSharpBuiltInTypeName[type] + (type.IsNullable() ? "?" : "") 
                : @namespace + (!type.IsGenericType ? type.Name :
                    (type.IsNullable() 
                        ? $"{GetFriendlyTypeName(underlyingType)}?" 
                        : $@"{type.GetNonGenericName()}<{type.GetGenericArguments()
                            .Select(x => x.GetFriendlyTypeName(includeNamespace)).Join(", ")}>"));
        }

        public static string GetFriendlyMethodName(this MethodInfo method, bool includeNamespace = false)
        {
            return method.Name + (method.IsGenericMethod ? $@"<{method.GetGenericArguments()
                .Select(x => x.GetFriendlyTypeName(includeNamespace)).Join(", ")}>" : "");
        }

        public static string GetFriendlyParameterName(this ParameterInfo parameter, bool includeNamespace = false)
        {
            return $"{parameter.ParameterType.GetFriendlyTypeName(includeNamespace)} {parameter.Name}";
        }

        public static string GetFriendlyPropertyName(this PropertyInfo property, bool includeNamespace = false)
        {
            return $"{property.PropertyType.GetFriendlyTypeName(includeNamespace)} {property.Name} " +
                   $"{{ {(property.CanRead ? "get; " : "")}{(property.CanWrite ? "set; " : "")}}}";
        }

        public static string GetFriendlyName(this Assembly assembly)
        {
            var name = assembly.GetName();
            return $"{name.Name} {name.Version}";
        }

        public static bool IsSystemAssembly(this Assembly assembly)
        {
            return assembly != null && (assembly.IsBclAssembly() || 
                assembly.FullName.StartsWith("System."));
        }

        public static bool IsBclAssembly(this Assembly assembly)
        {
            return assembly == typeof(string).Assembly;
        }

        public static bool IsBclType(this Type type)
        {
            return type.Assembly.IsBclAssembly();
        }

        public static bool IsBclMethod(this MethodInfo method)
        {
            return method.DeclaringType.IsBclType();
        }

        public static bool IsUnderNamespace(this Type type, string @namespace)
        {
            return type.Namespace == @namespace || type.Namespace.StartsWith($"{@namespace}.");
        }

        public static bool IsUnderNamespace<T>(this Type type, string relativeNamespace = null)
        {
            var compareType = typeof(T);
            if (type.Namespace.IsNullOrEmpty() || compareType.Namespace.IsNullOrEmpty()) return false;
            var compare = compareType.Namespace + (relativeNamespace.IsNotNullOrEmpty() 
                ? $".{relativeNamespace.Trim('.')}" : "");
            return type.Namespace == compare || type.Namespace.StartsWith($"{compare}.");
        }

        public static ConstructorInfo GetParameterlessConstructor(this Type type)
        {
            return type.GetConstructor(Type.EmptyTypes);
        }

        public static bool HasParameterlessConstructor(this Type type)
        {
            return type.GetParameterlessConstructor() != null;
        }

        public static bool HasAttribute<T>(this ParameterInfo parameter)
            where T : Attribute
        {
            return parameter.GetCustomAttributes<T>().Any();
        }

        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static Type GetUnderlyingNullableType(this Type type)
        {
            return type.IsNullable() ? Nullable.GetUnderlyingType(type) : type;
        }

        public static Type TryGetArrayElementType(this Type type)
        {
            return type.IsArray ? type.GetElementType() : null;
        }

        public static Type TryGetGenericListCastableElementType(this Type type)
        {
            if (!type.IsGenericType) return null;
            var typeDefinition = type.GetGenericTypeDefinition();
            return typeDefinition == typeof(List<>) ||
                   typeDefinition == typeof(IList<>) ||
                   typeDefinition == typeof(IEnumerable<>) ||
                   typeDefinition == typeof(ICollection<>)
                ? type.GenericTypeArguments[0] : null;
        }

        public static bool Is<T>(this Type type)
        {
            return type == typeof(T);
        }

        public static bool Is<T>(this Type type, bool includeNullable) where T : struct
        {
            return type.Is<T>() || (includeNullable && type.Is<T?>());
        }

        public static object WrapWithFormatException(this string value, Func<string, object> parse)
        {
            try
            {
                return parse(value);
            }
            catch (Exception exception)
            {
                throw new FormatException($"Unable to parse '{value}'.", exception);
            }
        }
        
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

        public static ParseResult<T> TryParseEnum<T>(this string value, TypeDescriptor type)
        {
            try
            {
                var result = (T)Enum.Parse(type.UnderlyingNullableType.Type, value, true);
                return ParseResult<T>.Succeeded(value, result);
            }
            catch
            {
                return ParseResult<T>.Failed(value, EnumParseError.ToFormat(value));
            }
        }

        public static ParseResult<T?> TryParseNullableEnum<T>(this string value, TypeDescriptor type) where T : struct 
        {
            if (value == null) return ParseResult<T?>.Succeeded(null, null);
            try
            {
                var result = (T?)Enum.Parse(type.UnderlyingNullableType.Type, value, true);
                return ParseResult<T?>.Succeeded(value, result);
            }
            catch
            {
                return ParseResult<T?>.Failed(value, EnumParseError.ToFormat(value));
            }
        }

        public const string CharParseError = "Char length {0} is invalid.";

        public static ParseResult<char> TryParseChar(this string value)
        {
            return value.Length == 1
                ? ParseResult<char>.Succeeded(value, value[0])
                : ParseResult<char>.Failed(value, CharParseError.ToFormat(value.Length));
        }
        
        public static ParseResult<char?> TryParseNullableChar(this string value)
        {
            if (value == null) return ParseResult<char?>.Succeeded(null, null);
            return value.Length == 1
                ? ParseResult<char?>.Succeeded(value, value[0])
                : ParseResult<char?>.Failed(value, CharParseError.ToFormat(value.Length));
        }

        public const string BoolParseError = "'{0}' is not a valid boolean. Must be 'true' or 'false'.";

        public static ParseResult<bool> TryParseBool(this string value)
        {
            bool parsedValue;
            return bool.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<bool>.Succeeded(value, parsedValue)
                : ParseResult<bool>.Failed(value, BoolParseError.ToFormat(value));
        }
        
        public static ParseResult<bool?> TryParseNullableBool(this string value)
        {
            if (value == null) return ParseResult<bool?>.Succeeded(null, null);
            bool parsedValue;
            return bool.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<bool?>.Succeeded(value, parsedValue)
                : ParseResult<bool?>.Failed(value, BoolParseError.ToFormat(value));
        }

        public const string SByteParseError = "'{0}' is not a valid signed byte. Must be an integer between -128 and 127.";

        public static ParseResult<sbyte> TryParseSByte(this string value)
        {
            sbyte parsedValue;
            return sbyte.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<sbyte>.Succeeded(value, parsedValue)
                : ParseResult<sbyte>.Failed(value, SByteParseError.ToFormat(value));
        }
        
        public static ParseResult<sbyte?> TryParseNullableSByte(this string value)
        {
            if (value == null) return ParseResult<sbyte?>.Succeeded(null, null);
            sbyte parsedValue;
            return sbyte.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<sbyte?>.Succeeded(value, parsedValue)
                : ParseResult<sbyte?>.Failed(value, SByteParseError.ToFormat(value));
        }

        public const string ByteParseError = "'{0}' is not a valid byte. Must be an integer between 0 and 255.";

        public static ParseResult<byte> TryParseByte(this string value)
        {
            byte parsedValue;
            return byte.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<byte>.Succeeded(value, parsedValue)
                : ParseResult<byte>.Failed(value, ByteParseError.ToFormat(value));
        }
        
        public static ParseResult<byte?> TryParseNullableByte(this string value)
        {
            if (value == null) return ParseResult<byte?>.Succeeded(null, null);
            byte parsedValue;
            return byte.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<byte?>.Succeeded(value, parsedValue)
                : ParseResult<byte?>.Failed(value, ByteParseError.ToFormat(value));
        }

        public const string Int16ParseError = "'{0}' is not a valid 16 bit integer. Must be an integer between -32,768 and 32,767.";

        public static ParseResult<short> TryParseInt16(this string value)
        {
            short parsedValue;
            return short.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<short>.Succeeded(value, parsedValue)
                : ParseResult<short>.Failed(value, Int16ParseError.ToFormat(value));
        }
        
        public static ParseResult<short?> TryParseNullableInt16(this string value)
        {
            if (value == null) return ParseResult<short?>.Succeeded(null, null);
            short parsedValue;
            return short.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<short?>.Succeeded(value, parsedValue)
                : ParseResult<short?>.Failed(value, Int16ParseError.ToFormat(value));
        }

        public const string UInt16ParseError = "'{0}' is not a valid 16 bit unsigned integer. Must be an integer between 0 and 65,535.";

        public static ParseResult<ushort> TryParseUInt16(this string value)
        {
            ushort parsedValue;
            return ushort.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<ushort>.Succeeded(value, parsedValue)
                : ParseResult<ushort>.Failed(value, UInt16ParseError.ToFormat(value));
        }
        
        public static ParseResult<ushort?> TryParseNullableUInt16(this string value)
        {
            if (value == null) return ParseResult<ushort?>.Succeeded(null, null);
            ushort parsedValue;
            return ushort.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<ushort?>.Succeeded(value, parsedValue)
                : ParseResult<ushort?>.Failed(value, UInt16ParseError.ToFormat(value));
        }

        public const string Int32ParseError = "'{0}' is not a valid 32 bit integer. Must be an integer between -2,147,483,648 and 2,147,483,647.";
 
        public static ParseResult<int> TryParseInt32(this string value)
        {
            int parsedValue;
            return int.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<int>.Succeeded(value, parsedValue)
                : ParseResult<int>.Failed(value, Int32ParseError.ToFormat(value));
        }

        public static ParseResult<int?> TryParseNullableInt32(this string value)
        {
            if (value == null) return ParseResult<int?>.Succeeded(null, null);
            int parsedValue;
            return int.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<int?>.Succeeded(value, parsedValue)
                : ParseResult<int?>.Failed(value, Int32ParseError.ToFormat(value));
        }

        public const string UInt32ParseError = "'{0}' is not a valid 32 bit unsigned integer. Must be an integer between 0 and 4,294,967,295.";

        public static ParseResult<uint> TryParseUInt32(this string value)
        {
            uint parsedValue;
            return uint.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<uint>.Succeeded(value, parsedValue)
                : ParseResult<uint>.Failed(value, UInt32ParseError.ToFormat(value));
        }
        
        public static ParseResult<uint?> TryParseNullableUInt32(this string value)
        {
            if (value == null) return ParseResult<uint?>.Succeeded(null, null);
            uint parsedValue;
            return uint.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<uint?>.Succeeded(value, parsedValue)
                : ParseResult<uint?>.Failed(value, UInt32ParseError.ToFormat(value));
        }

        public const string Int64ParseError = "'{0}' is not a valid 64 bit integer. Must be an integer between -9,223,372,036,854,775,808 and 9,223,372,036,854,775,807.";

        public static ParseResult<long> TryParseInt64(this string value)
        {
            long parsedValue;
            return long.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<long>.Succeeded(value, parsedValue)
                : ParseResult<long>.Failed(value, Int64ParseError.ToFormat(value));
        }
        
        public static ParseResult<long?> TryParseNullableInt64(this string value)
        {
            if (value == null) return ParseResult<long?>.Succeeded(null, null);
            long parsedValue;
            return long.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<long?>.Succeeded(value, parsedValue)
                : ParseResult<long?>.Failed(value, Int64ParseError.ToFormat(value));
        }

        public const string UInt64ParseError = "'{0}' is not a valid 64 bit unsigned integer. Must be an integer between 0 and 18,446,744,073,709,551,615.";

        public static ParseResult<ulong> TryParseUInt64(this string value)
        {
            ulong parsedValue;
            return ulong.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<ulong>.Succeeded(value, parsedValue)
                : ParseResult<ulong>.Failed(value, UInt64ParseError.ToFormat(value));
        }
        
        public static ParseResult<ulong?> TryParseNullableUInt64(this string value)
        {
            if (value == null) return ParseResult<ulong?>.Succeeded(null, null);
            ulong parsedValue;
            return ulong.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<ulong?>.Succeeded(value, parsedValue)
                : ParseResult<ulong?>.Failed(value, UInt64ParseError.ToFormat(value));
        }

        public const string SingleParseError = "'{0}' is not a valid floating point number. Must be a real number between -3.4 x 10^38 and +3.4 x 10^38.";

        public static ParseResult<float> TryParseSingle(this string value)
        {
            float parsedValue;
            return float.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<float>.Succeeded(value, parsedValue)
                : ParseResult<float>.Failed(value, SingleParseError.ToFormat(value));
        }
        
        public static ParseResult<float?> TryParseNullableSingle(this string value)
        {
            if (value == null) return ParseResult<float?>.Succeeded(null, null);
            float parsedValue;
            return float.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<float?>.Succeeded(value, parsedValue)
                : ParseResult<float?>.Failed(value, SingleParseError.ToFormat(value));
        }

        public const string DoubleParseError = "'{0}' is not a valid floating point number. Must be a real number between +/- 5.0 x 10^−324 to +/- 1.7 x 10^308.";

        public static ParseResult<double> TryParseDouble(this string value)
        {
            double parsedValue;
            return double.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<double>.Succeeded(value, parsedValue)
                : ParseResult<double>.Failed(value, DoubleParseError.ToFormat(value));
        }
        
        public static ParseResult<double?> TryParseNullableDouble(this string value)
        {
            if (value == null) return ParseResult<double?>.Succeeded(null, null);
            double parsedValue;
            return double.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<double?>.Succeeded(value, parsedValue)
                : ParseResult<double?>.Failed(value, DoubleParseError.ToFormat(value));
        }

        public const string DecimalParseError = "'{0}' is not a valid floating point number. Must be a real number between +/- 5.0 x 10^−324 to +/- 1.7 x 10^308.";

        public static ParseResult<decimal> TryParseDecimal(this string value)
        {
            decimal parsedValue;
            return decimal.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<decimal>.Succeeded(value, parsedValue)
                : ParseResult<decimal>.Failed(value, DecimalParseError.ToFormat(value));
        }

        public static ParseResult<decimal?> TryParseNullableDecimal(this string value)
        {
            if (value == null) return ParseResult<decimal?>.Succeeded(null, null);
            decimal parsedValue;
            return decimal.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<decimal?>.Succeeded(value, parsedValue)
                : ParseResult<decimal?>.Failed(value, DecimalParseError.ToFormat(value));
        }

        public const string DateTimeParseError = "'{0}' is not a valid datetime.";

        public static ParseResult<DateTime> TryParseDateTime(this string value)
        {
            DateTime parsedValue;
            return DateTime.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<DateTime>.Succeeded(value, parsedValue)
                : ParseResult<DateTime>.Failed(value, DateTimeParseError.ToFormat(value));
        }

        public static ParseResult<DateTime?> TryParseNullableDateTime(this string value)
        {
            if (value == null) return ParseResult<DateTime?>.Succeeded(null, null);
            DateTime parsedValue;
            return DateTime.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<DateTime?>.Succeeded(value, parsedValue)
                : ParseResult<DateTime?>.Failed(value, DateTimeParseError.ToFormat(value));
        }

        public const string GuidParseError = "'{0}' is not a valid uuid. Should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).";

        public static ParseResult<Guid> TryParseGuid(this string value)
        {
            Guid parsedValue;
            return Guid.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<Guid>.Succeeded(value, parsedValue)
                : ParseResult<Guid>.Failed(value, GuidParseError.ToFormat(value));
        }
        
        public static ParseResult<Guid?> TryParseNullableGuid(this string value)
        {
            if (value == null) return ParseResult<Guid?>.Succeeded(null, null);
            Guid parsedValue;
            return Guid.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<Guid?>.Succeeded(value, parsedValue)
                : ParseResult<Guid?>.Failed(value, GuidParseError.ToFormat(value));
        }

        public const string TimeSpanParseError = "'{0}' is not a valid time span. Must be in the format 'h:m:s'.";

        public static ParseResult<TimeSpan> TryParseTimeSpan(this string value)
        {
            TimeSpan parsedValue;
            return TimeSpan.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<TimeSpan>.Succeeded(value, parsedValue)
                : ParseResult<TimeSpan>.Failed(value, TimeSpanParseError.ToFormat(value));
        }

        public static ParseResult<TimeSpan?> TryParseNullableTimeSpan(this string value)
        {
            if (value == null) return ParseResult<TimeSpan?>.Succeeded(null, null);
            TimeSpan parsedValue;
            return TimeSpan.TryParse(value.ToLower(), out parsedValue)
                ? ParseResult<TimeSpan?>.Succeeded(value, parsedValue)
                : ParseResult<TimeSpan?>.Failed(value, TimeSpanParseError.ToFormat(value));
        }
        
        public const string UriParseError = "'{0}' is not valid url.";

        public static ParseResult<Uri> TryParseUri(this string value)
        {
            try
            {
                return ParseResult<Uri>.Succeeded(value, new Uri(value));
            }
            catch
            {
                return ParseResult<Uri>.Failed(value, UriParseError.ToFormat(value));
            }
        }
    }
}
