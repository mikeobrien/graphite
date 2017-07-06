using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Graphite.Extensions;
using Should;
using Should.Core.Assertions;
using Should.Core.Exceptions;

namespace Tests.Common
{
    public static class Should
    {
        public static T Throw<T>(Action action) where T : Exception
        {
            return Assert.Throws<T>(() => action());
        }

        public static void NotThrow(Action action)
        {
            Assert.DoesNotThrow(() => action());
        }
    }
    
    public static class FluentAssertions
    {
        public static void ShouldBeInteger(this string value)
        {
            int output;
            int.TryParse(value, out output).ShouldEqual(true, $"{value} is not an integer.");
        }

        public static void ShouldEndWith(this string value, string expected)
        {
            value.EndsWith(expected).ShouldEqual(true, $"'{value}' does not end with '{expected}'.");
        }

        public static void ShouldOnlyContain<T>(
            this IEnumerable<T> source, params T[] items)
        {
            source.ShouldOnlyContain((IEnumerable<T>)
                (items ?? new List<T> { default(T) }.ToArray()));
        }

        public static void ShouldOnlyContainTypes<T>(
            this IEnumerable<object> source)
        {
            source.Select(x => x.GetType()).ShouldOnlyContain(new [] { typeof(T) });
        }

        public static void ShouldOnlyContainTypes<T1, T2>(
            this IEnumerable<object> source)
        {
            source.Select(x => x.GetType()).ShouldOnlyContain(new[]
            {
                typeof(T1), typeof(T2)
            });
        }

        public static void ShouldOnlyContainTypes<T1, T2, T3>(
            this IEnumerable<object> source)
        {
            source.Select(x => x.GetType()).ShouldOnlyContain(new[]
            {
                typeof(T1), typeof(T2), typeof(T3)
            });
        }

        public static void ShouldOnlyContainTypes<T1, T2, T3, T4>(
            this IEnumerable<object> source)
        {
            source.Select(x => x.GetType()).ShouldOnlyContain(new[]
            {
                typeof(T1), typeof(T2), typeof(T3), typeof(T4)
            });
        }

        public static void ShouldOnlyContainTypes<T1, T2, T3, T4, T5>(
            this IEnumerable<object> source)
        {
            source.Select(x => x.GetType()).ShouldOnlyContain(new[]
            {
                typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5)
            });
        }

        public static void ShouldOnlyContain<T>(
            this IEnumerable<T> source, IEnumerable<T> compare)
        {
            if ((source == null && compare == null) || 
                (source != null && !source.Any() && 
                 compare != null && !compare.Any())) return;
            source.ToArray().ShouldEqual(compare.ToArray());
        }

        public static void ShouldContain<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            source.Any(predicate).ShouldBeTrue();
        }

        public static void ShouldNotContain<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            source.Any(predicate).ShouldBeFalse();
        }

        public static void ShouldEqual<T>(this Type type)
        {
            type.ShouldEqual(typeof(T));
        }

        public static string ShouldContainAll(this string actual, params string[] expected)
        {
            return actual.ShouldContainAll(StringComparison.Ordinal, expected);
        }

        public static string ShouldContainAllIgnoreCase(this string actual, params string[] expected)
        {
            return actual.ShouldContainAll(StringComparison.OrdinalIgnoreCase, expected);
        }

        private static string ShouldContainAll(this string actual,
            StringComparison comparison, params string[] expected)
        {
            expected.Where(x => x != null)
                .ForEach(x => actual.Contains(x, comparison)
                .ShouldBeTrue($"\"{x}\" not found in \"{actual}\""));
            return actual;
        }

        public class ShouldDsl<T>
        {
            private readonly T _source;

            public ShouldDsl(T source)
            {
                _source = source;
            }

            public TException Throw<TException>(Action<T> action, string message = null)
                where TException : Exception
            {
                return (TException)Throw(typeof(TException), action, message);
            }

            public Exception Throw(Type type, Action<T> action, string message = null)
            {
                var exception = Assert.Throws(type, () => action(_source));
                if (message != null) exception.Message.ShouldEqual(message);
                return exception;
            }

            public async Task<TException> Throw<TException>(Func<T, Task> action, string message = null)
                where TException : Exception
            {
                try
                {
                    await action(_source);
                    throw new AssertException($"Expected to throw {typeof(TException).Name} but none was thrown.");
                }
                catch (Exception exception)
                {
                    var actualException = exception is AggregateException ? exception.InnerException : exception;
                    if (!(actualException is TException))
                    {
                        throw new Exception($"Expected to throw {typeof(TException).Name} " +
                            $"but threw {exception.GetType().Name}.", actualException);
                    }
                    if (message != null) exception.Message.ShouldEqual(message);
                    return (TException)actualException;
                }
            }

            public void NotThrow(Action<T> action)
            {
                Assert.DoesNotThrow(() => action(_source));
            }

            public TResult NotThrow<TResult>(Func<T, TResult> action)
            {
                var result = default(TResult);
                Assert.DoesNotThrow(() => result = action(_source));
                return result;
            }
        }

        public static ShouldDsl<T> Should<T>(this T source)
        {
            return new ShouldDsl<T>(source);
        }
    }
}
