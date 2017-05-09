using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Graphite.Actions;

namespace Tests.Common.Fakes
{
    public class TestActionMethodSource : IActionMethodSource
    {
        private readonly List<ActionMethod> _actionMethods = new List<ActionMethod>();

        public bool AppliesToCalled { get; set; }
        public Func<ActionMethodSourceContext, bool> AppliesToFunc { get; set; }
        public ActionMethodSourceContext AppliesToContext { get; set; }
        public bool GetActionMethodsCalled { get; set; }
        public ActionMethodSourceContext GetActionMethodsContext { get; set; }

        public ActionMethod Add<T>(Expression<Func<T, object>> method)
        {
            var actionMethod = method.ToActionMethod();
            _actionMethods.Add(actionMethod);
            return actionMethod;
        }

        public bool AppliesTo(ActionMethodSourceContext context)
        {
            AppliesToCalled = true;
            AppliesToContext = context;
            return AppliesToFunc?.Invoke(context) ?? true;
        }

        public IEnumerable<ActionMethod> GetActionMethods(
            ActionMethodSourceContext context)
        {
            GetActionMethodsCalled = true;
            GetActionMethodsContext = context;
            return _actionMethods;
        }
    }
}
