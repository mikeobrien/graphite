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
            public List<string> Params { get; set; } = new List<string>();

            public void Method()
            {
                CalledMethod = nameof(Method);
            }

            public void MethodWithParameter(string param)
            {
                Params.Add(param);
                CalledMethod = nameof(MethodWithParameter);
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
            .CreateWithExpression<InvokerMethods, object[], object>(x => x
            .Add(t => t.Method(), null, null)
            .Add(t => t.AsyncMethod(), null, null)
            .Add(t => t.MethodWithParameter(null), new[] { "param" }, null)
            .Add(t => t.MethodWithParameters(null, null), new[] { "param1", "param2" }, null)
            .Add(t => t.MethodWithReturn(), null, "response")
            .Add(t => t.AsyncMethodWithReturn(), null, "response")
            .Add(t => t.MethodWithReturnAndParameter(null), new [] { "param" }, "response")
            .Add(t => t.MethodWithReturnAndParameters(null, null), new[] { "param1", "param2" }, "response"));

        [TestCaseSource(nameof(InvokerTestCases))]
        public async Task Should_invoke_method(LambdaExpression expression, 
            object[] parameters, object response)
        {
            var type = new TypeCache().GetTypeDescriptor(typeof(InvokerMethods));
            var method = type.Methods.FirstOrDefault(x => 
                x.MethodInfo == expression.GetMethodInfo());
            var invoker = method.GenerateAsyncInvoker(type);
            var instance = new InvokerMethods();

            var result = await invoker(instance, parameters);

            instance.CalledMethod.ShouldEqual(method.Name);
            if (parameters != null) instance.Params.ShouldOnlyContain(parameters);
            result.ShouldEqual(response);
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
