using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Graphite;
using Graphite.Actions;
using Graphite.Reflection;
using NUnit.Framework;
using Tests.Common;
using Tests.Unit.Actions.ActionSource;

namespace Tests.Unit.Actions
{
    namespace ActionSource
    {
        public class Handler
        {
            public void NotAnAction() { }
            public void Get() { }
            public void Get_Param() { }
            public void GetGeneric<T>() { }

            public class NestedHandler
            {
                public void Post() { }
            }
        }

        public class SomeClass
        {
            public void Get() { }
        }
    }

    [TestFixture]
    public class DefaultActionMethodSourceTests
    {
        private List<ActionMethod> _actionMethods;

        [OneTimeSetUp]
        public void Setup()
        {
            _actionMethods = new DefaultActionMethodSource(
                new Configuration
                {
                    Assemblies = { Assembly.GetExecutingAssembly() }
                }, new TypeCache())
                .GetActionMethods().ToList();
        }

        [Test]
        public void Should_return_matching_handlers()
        {
            Should_contain_action_method<Handler>(nameof(Handler.Get));
            Should_contain_action_method<Handler>(nameof(Handler.Get_Param));
            Should_contain_action_method<Handler.NestedHandler>(nameof(Handler.NestedHandler.Post));
        }

        [Test]
        public void Should_not_return_non_matching_handlers()
        {
            Should_not_contain_action_method<SomeClass>(nameof(SomeClass.Get));
        }

        [Test]
        public void Should_not_return_non_matching_actions()
        {
            Should_not_contain_action_method<Handler>(nameof(Handler.NotAnAction));
        }

        [Test]
        public void Should_not_return_bcl_method_actions()
        {
            Should_not_contain_action_method<Handler>(nameof(GetType));
            Should_not_contain_action_method<Handler>(nameof(GetHashCode));
        }

        [Test]
        public void Should_not_return_generic_methods()
        {
            Should_not_contain_action_method<Handler>(nameof(Handler.GetGeneric));
        }

        private void Should_contain_action_method<T>(string methodName)
        {
            _actionMethods.ShouldContain(x => x.HandlerTypeDescriptor.Type == typeof(T) &&
                x.MethodDescriptor.MethodInfo == typeof(T).GetMethod(methodName));
        }

        private void Should_not_contain_action_method<T>(string methodName)
        {
            _actionMethods.ShouldNotContain(x => x.HandlerTypeDescriptor.Type == typeof(T) &&
                x.MethodDescriptor.MethodInfo == typeof(T).GetMethod(methodName));
        }
    }
}
