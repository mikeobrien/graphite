using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Graphite;
using Graphite.Actions;
using Graphite.Extensions;
using Graphite.Reflection;
using NUnit.Framework;
using Should;
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

        [TestCase("", false)]
        [TestCase("Fark", false)]
        [TestCase("Handler", true)]
        [TestCase("FarkHandler", true)]
        public void Should_match_handler_name(string name, bool matches)
        {
            DefaultActionMethodSource.DefaultHandlerNameConvention.IsMatch(name).ShouldEqual(matches);
        }

        [TestCase("", false)]
        [TestCase("Fark", false)]
        [TestCase("GetFark", true)]
        [TestCase("Get", true)]
        [TestCase("Post", true)]
        [TestCase("Put", true)]
        [TestCase("Delete", true)]
        [TestCase("Options", true)]
        [TestCase("Head", true)]
        [TestCase("Trace", true)]
        [TestCase("Connect", true)]
        public void Should_match_method_name(string name, bool matches)
        {
            DefaultActionMethodSource.DefaultActionNameConvention(
                    new Configuration()).IsMatch(name)
                .ShouldEqual(matches);
        }
        
        [TestCase("", null)]
        [TestCase("GetFark", "Get")]
        [TestCase("Get", "Get")]
        [TestCase("Post", "Post")]
        [TestCase("Put", "Put")]
        [TestCase("Delete", "Delete")]
        [TestCase("Options", "Options")]
        [TestCase("Head", "Head")]
        [TestCase("Trace", "Trace")]
        [TestCase("Connect", "Connect")]
        public void Should_return_method_name(string name, string expected)
        {
            name.MatchGroupValue(DefaultActionMethodSource
                .DefaultActionNameConvention(new Configuration()),
                    DefaultActionMethodSource.HttpMethodGroupName)
                .ShouldEqual(expected);
        }

        [TestCase("", null)]
        [TestCase("GetFark", "Fark")]
        [TestCase("Get_Fark", "_Fark")]
        [TestCase("Get_Fark_Farker", "_Fark_Farker")]
        [TestCase("GetFark_Farker", "Fark_Farker")]
        public void Should_return_segments(string name, string expected)
        {
            name.MatchGroupValue(DefaultActionMethodSource
                        .DefaultActionNameConvention(new Configuration()),
                    DefaultActionMethodSource.ActionSegmentsGroupName)
                .ShouldEqual(expected);
        }
    }
}
