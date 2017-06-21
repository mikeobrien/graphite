using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Graphite.Extensions;

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
                .Cast<DebuggableAttribute>().Any(x => x.IsJITTrackingEnabled);
        }

        public static string GetGenericTypeBaseName(this Type type)
        {
            var index = type.Name.IndexOf('`');
            return index > 0 ? type.Name.Remove(index) : type.Name;
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
                        : $@"{type.GetGenericTypeBaseName()}<{type.GetGenericArguments()
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

        public static bool IsSimpleType(this Type type)
        {
            Func<Type, bool> isSimpleType = x => x.IsPrimitive || x.IsEnum || x == typeof(string) || x == typeof(decimal) ||
                 x == typeof(DateTime) || x == typeof(TimeSpan) || x == typeof(Guid) || x == typeof(Uri);
            return isSimpleType(type) || (type.IsNullable() && isSimpleType(Nullable.GetUnderlyingType(type)));
        }

        public static object ParseSimpleType(this string value, TypeDescriptor type)
        {
            if (type.Type.Is<string>() || value == null) return value;
            if (type.UnderlyingNullableType.Type.IsEnum) return value
                .WrapWithFormatException(x => Enum.Parse(type
                    .UnderlyingNullableType.Type, value, true));
            if (type.Type.Is<char>(true))
            {
                if (value.Length == 1) return value[0];
                throw new FormatException($"Char length {value.Length} is invalid.");
            }
            if (type.Type.Is<bool>(true)) return bool.Parse(value.ToLower());
            if (type.Type.Is<sbyte>(true)) return sbyte.Parse(value);
            if (type.Type.Is<byte>(true)) return byte.Parse(value);
            if (type.Type.Is<short>(true)) return short.Parse(value);
            if (type.Type.Is<ushort>(true)) return ushort.Parse(value);
            if (type.Type.Is<int>(true)) return int.Parse(value);
            if (type.Type.Is<uint>(true)) return uint.Parse(value);
            if (type.Type.Is<long>(true)) return long.Parse(value);
            if (type.Type.Is<ulong>(true)) return ulong.Parse(value);
            if (type.Type.Is<float>(true)) return float.Parse(value);
            if (type.Type.Is<double>(true)) return double.Parse(value);
            if (type.Type.Is<decimal>(true)) return decimal.Parse(value);
            if (type.Type.Is<DateTime>(true)) return DateTime.Parse(value);
            if (type.Type.Is<Guid>(true)) return Guid.Parse(value);
            if (type.Type.Is<TimeSpan>(true)) return TimeSpan.Parse(value);
            if (type.Type.Is<Uri>()) return value.WrapWithFormatException(x => new Uri(x));
            if (type.Type.Is<IntPtr>(true)) return new IntPtr(long.Parse(value));
            if (type.Type.Is<UIntPtr>(true)) return new UIntPtr(ulong.Parse(value));
            throw new ArgumentException($"Type '{type}' is not a simple type.");
        }
    }
}
