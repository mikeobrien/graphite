using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Graphite.Actions;
using Graphite.Extensions;
using Graphite.Reflection;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Actions
{
    [TestFixture]
    public class AsyncInvokerTests
    {
        public class InvokerMethods
        {
            public string CalledMethod { get; set; }
            public List<object> Params { get; set; } = new List<object>();

            public void Method()
            {
                CalledMethod = nameof(Method);
            }

            public void MethodWithParameter(string param)
            {
                Params.Add(param);
                CalledMethod = nameof(MethodWithParameter);
            }

            public void MethodWithNonNullableParameter(int param)
            {
                Params.Add(param);
                CalledMethod = nameof(MethodWithNonNullableParameter);
            }

            public void MethodWithParameters(string param1, string param2)
            {
                Params.AddRange(new[] { param1, param2 });
                CalledMethod = nameof(MethodWithParameters);
            }

            public string MethodWithReturn()
            {
                CalledMethod = nameof(MethodWithReturn);
                return "response";
            }

            public string MethodWithReturnAndParameter(string param)
            {
                Params.Add(param);
                CalledMethod = nameof(MethodWithReturnAndParameter);
                return "response";
            }

            public string MethodWithReturnAndParameters(string param1, string param2)
            {
                Params.AddRange(new [] { param1, param2 });
                CalledMethod = nameof(MethodWithReturnAndParameters);
                return "response";
            }

            public Task AsyncMethod()
            {
                CalledMethod = nameof(AsyncMethod);
                return Task.Delay(10);
            }

            public async Task<string> AsyncMethodWithReturn()
            {
                CalledMethod = nameof(AsyncMethodWithReturn);
                return "response";
            }
        }

        public static readonly object[][] InvokerTestCases = TestCaseSource
            .CreateWithExpression<InvokerMethods, object[], object[], object>(x => x
            .Add(t => t.Method(), null, null, null)
            .Add(t => t.AsyncMethod(), null, null, null)
            .Add(t => t.MethodWithParameter(null), new[] { "param" }, new[] { "param" }, null)
            .Add(t => t.MethodWithNonNullableParameter(0), new object[] { 0 }, new object[] { 0 }, null)
            .Add(t => t.MethodWithNonNullableParameter(0), new object[] { null }, new object[] { 0 }, null)
            .Add(t => t.MethodWithParameters(null, null), new[] { "param1", "param2" }, 
                new[] { "param1", "param2" }, null)
            .Add(t => t.MethodWithReturn(), null, null, "response")
            .Add(t => t.AsyncMethodWithReturn(), null, null, "response")
            .Add(t => t.MethodWithReturnAndParameter(null), new [] { "param" }, new[] { "param" }, "response")
            .Add(t => t.MethodWithReturnAndParameters(null, null), new[] { "param1", "param2" }, 
                new[] { "param1", "param2" }, "response"));

        [TestCaseSource(nameof(InvokerTestCases))]
        public async Task Should_invoke_method(LambdaExpression expression, 
            object[] parameters, object[] expected, object response)
        {
            var type = new TypeCache().GetTypeDescriptor(typeof(InvokerMethods));
            var method = type.Methods.FirstOrDefault(x => 
                x.MethodInfo == expression.GetMethodInfo());
            var invoker = method.GenerateAsyncInvoker(type);
            var instance = new InvokerMethods();

            var result = await invoker(instance, parameters);

            instance.CalledMethod.ShouldEqual(method.Name);
            if (parameters != null) instance.Params.ShouldOnlyContain(expected);
            result.ShouldEqual(response);
        }

        [Test]
        public async Task Should_throw_informative_message_when_argument_cannot_be_cast_to_parameter_type()
        {
            var type = new TypeCache().GetTypeDescriptor(typeof(InvokerMethods));
            var method = type.Methods.FirstOrDefault(x =>
                x.MethodInfo.Name == nameof(InvokerMethods.MethodWithNonNullableParameter));
            var invoker = method.GenerateAsyncInvoker(type);
            var instance = new InvokerMethods();

            var message = invoker.Should().Throw<ParameterInvalidCastException>(
                async x => await invoker(instance, new object[] {"fark"})).Result.Message;

            message.ShouldContain("param");
            message.ShouldContain("String");
            message.ShouldContain("Int32");
        }

        [Test]
        public void Should_invoke_method_faster_than_reflection()
        {
            var type = new TypeCache().GetTypeDescriptor(typeof(InvokerMethods));
            var method = type.Methods.First(x => x.Name == nameof(InvokerMethods.MethodWithReturnAndParameters));
            var invoke = method.GenerateAsyncInvoker(type);
            var instance = new InvokerMethods();
            var parameters = new object[] { "param1", "param2" };

            var comparison = PerformanceComparison.InTicks(10000);

            comparison.AddCase("Native", () => instance
                    .MethodWithReturnAndParameters((string)parameters[0], (string)parameters[1]));

            var compiledCase = comparison.AddCase("Compiled expression", () => invoke(instance, parameters));
            var reflectionCase = comparison.AddCase("Reflection", () => method.MethodInfo.Invoke(instance, parameters));

            comparison.Run();

            compiledCase.Average.ShouldBeLessThan(reflectionCase.Average);
        }
    }
}
