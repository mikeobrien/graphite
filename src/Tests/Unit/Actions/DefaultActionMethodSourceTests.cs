using System;
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
        public List<ActionMethod> GetActions(Action<Configuration> config = null)
        {
            var configuration = new Configuration
            {
                Assemblies = { Assembly.GetExecutingAssembly() }
            };
            config?.Invoke(configuration);
            return new DefaultActionMethodSource(configuration, new TypeCache())
                .GetActionMethods().ToList();
        }

        [Test]
        public void Should_return_matching_handlers()
        {
            var actions = GetActions();
            Should_contain_action_method<Handler>(actions, nameof(Handler.Get));
            Should_contain_action_method<Handler>(actions, nameof(Handler.Get_Param));
            Should_contain_action_method<Handler.NestedHandler>(actions, nameof(Handler.NestedHandler.Post));
        }

        [Test]
        public void Should_not_return_filtered_handlers()
        {
            var actions = GetActions(x => x.ActionFilter = (c, m) => m.DeclaringType.Name != "Handler");
            Should_not_contain_action_method<Handler>(actions, nameof(Handler.Get));
            Should_not_contain_action_method<Handler>(actions, nameof(Handler.Get_Param));
            Should_contain_action_method<Handler.NestedHandler>(actions, nameof(Handler.NestedHandler.Post));
            Should_not_contain_action_method<Handler>(actions, nameof(Handler.NotAnAction));
            Should_not_contain_action_method<SomeClass>(actions, nameof(SomeClass.Get));
        }

        [Test]
        public void Should_not_return_filtered_actions()
        {
            var actions = GetActions(x => x.ActionFilter = (c, m) => !m.Name.Contains("Param"));
            Should_contain_action_method<Handler>(actions, nameof(Handler.Get));
            Should_not_contain_action_method<Handler>(actions, nameof(Handler.Get_Param));
            Should_contain_action_method<Handler.NestedHandler>(actions, nameof(Handler.NestedHandler.Post));
            Should_not_contain_action_method<Handler>(actions, nameof(Handler.NotAnAction));
            Should_not_contain_action_method<SomeClass>(actions, nameof(SomeClass.Get));
        }

        [Test]
        public void Should_not_return_non_matching_handlers()
        {
            var actions = GetActions();
            Should_not_contain_action_method<SomeClass>(actions, nameof(SomeClass.Get));
        }

        [Test]
        public void Should_not_return_non_matching_actions()
        {
            var actions = GetActions();
            Should_not_contain_action_method<Handler>(actions, nameof(Handler.NotAnAction));
        }

        [Test]
        public void Should_not_return_bcl_method_actions()
        {
            var actions = GetActions();
            Should_not_contain_action_method<Handler>(actions, nameof(GetType));
            Should_not_contain_action_method<Handler>(actions, nameof(GetHashCode));
        }

        [Test]
        public void Should_not_return_generic_methods()
        {
            var actions = GetActions();
            Should_not_contain_action_method<Handler>(actions, nameof(Handler.GetGeneric));
        }

        private void Should_contain_action_method<T>(List<ActionMethod> actions, string methodName)
        {
            actions.ShouldContain(x => x.HandlerTypeDescriptor.Type.IsType<T>() &&
                x.MethodDescriptor.MethodInfo == typeof(T).GetMethod(methodName));
        }

        private void Should_not_contain_action_method<T>(List<ActionMethod> actions, string methodName)
        {
            actions.ShouldNotContain(x => x.HandlerTypeDescriptor.Type.IsType<T>() &&
                x.MethodDescriptor.MethodInfo == typeof(T).GetMethod(methodName));
        }
    }
}
