using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Actions;
using Graphite.DependencyInjection;

namespace Graphite.Behaviors
{
    public class BehaviorChain : Stack<Type>, IBehaviorChain
    {
        private readonly Type _defaultBehavior;
        private readonly IContainer _container;
        private readonly Stack<Type> _behaviors;

        public BehaviorChain(Configuration configuration,
            ActionDescriptor actionDescriptor, IContainer container)
        {
            _defaultBehavior = configuration.DefaultBehavior;
            _container = container;
            _behaviors = new Stack<Type>(actionDescriptor.Behaviors
                .Select(x => x.Type).Reverse());
        }

        public async Task<HttpResponseMessage> InvokeNext()
        {
            while (true)
            {
                if (_behaviors.Count == 0)
                    return await _container.GetInstance<IBehavior>(_defaultBehavior).Invoke();

                var behaviorType = _behaviors.Pop();
                var behavior = _container.GetInstance<IBehavior>(
                    behaviorType, Dependency.For<IBehaviorChain>(this));

                if (behavior.ShouldRun()) return await behavior.Invoke();
            }
        }
    }
}