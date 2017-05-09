using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Graphite.Actions;

namespace Tests.Common.Fakes
{
    public class TestActionMethodSource : IActionMethodSource
    {
        private readonly List<ActionMethod> _actionMethods = new List<ActionMethod>();

        public bool AppliesCalled { get; set; }
        public Func<bool> AppliesFunc { get; set; }
        public bool GetActionMethodsCalled { get; set; }

        public ActionMethod Add<T>(Expression<Func<T, object>> method)
        {
            var actionMethod = ActionMethod.From(method);
            _actionMethods.Add(actionMethod);
            return actionMethod;
        }

        public bool Applies()
        {
            AppliesCalled = true;
            return AppliesFunc?.Invoke() ?? true;
        }

        public IEnumerable<ActionMethod> GetActionMethods()
        {
            GetActionMethodsCalled = true;
            return _actionMethods;
        }
    }
}
