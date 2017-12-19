using System;
using System.Linq;
using Graphite.Extensions;
using Graphite.Reflection;

namespace Graphite.Binding
{
    public class ParsedValueMapper
    {
        public MapResult Map<T>(ValueMapperContext context, Func<string, ParseResult<T>> map)
        {
            var results = context.Values.Select(x => x is string ? map((string)x) : Cast<T>(x)).ToList();

            var failed = results.Where(x => !x.Success).ToList();

            if (failed.Count > 0)
            {
                var failedValues = failed.Select(x => $"'{x.Original}'").Join(", ");
                var failureMessages = failed.Select(x => x.ErrorMessage).Distinct().Join(" ");

                var message = $"{failedValues} {(failed.Count == 1 ? "is" : "are")} " + 
                              $"not formatted correctly. {failureMessages}";

                return MapResult.Failure(context.ForParameter
                    ? $"Parameter '{context.Parameter.Name}' value {message}"
                    : (context.ForPropertyOfParameter
                        ? $"Parameter '{context.Parameter.Name}.{context.Property.Name}' value {message}"
                        : (context.ForProperty
                            ? $"Property '{context.Property.Name}' value {message}"
                            :"Value {message}")));
            }
            
            var values = results.Select(x => x.Result);

            return MapResult.Success(
                context.Type.IsArray
                    ? values.ToArray()
                    : (context.Type.IsGenericListCastable
                        ? (object)values.ToList()
                        : values.FirstOrDefault()));
        }

        private ParseResult<T> Cast<T>(object value)
        {
            try
            {
                return ParseResult<T>.Succeeded(value?.ToString(), (T)value);
            }
            catch
            {
                var message = value != null
                    ? $"Could not cast type {value.GetType().FullName} to {typeof(T)}."
                    : $"Type {typeof(T)} is not nullable.";
                
                return ParseResult<T>.Failed(value?.ToString(), message);
            }
        }
    }
}