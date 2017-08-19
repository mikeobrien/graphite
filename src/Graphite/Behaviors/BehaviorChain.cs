using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Actions;
using Graphite.DependencyInjection;
using Graphite.Extensibility;

namespace Graphite.Behaviors
{
    public class BehaviorChain : Stack<Type>, IBehaviorChain
    {
        private readonly Type _defaultBehavior;
        private readonly IContainer _container;
        private readonly Stack<Plugin<IBehavior>> _behaviors;

        public BehaviorChain(Configuration configuration,
            ActionDescriptor actionDescriptor, IContainer container)
        {
            _defaultBehavior = configuration.DefaultBehavior;
            _container = container;
            _behaviors = new Stack<Plugin<IBehavior>>(actionDescriptor.Behaviors.Reverse());
        }

        public async Task<HttpResponseMessage> InvokeNext()
        {
            while (true)
            {
                if (_behaviors.Count == 0)
                    return await _container.GetInstance<IBehavior>(_defaultBehavior).Invoke();

                var plugin = _behaviors.Pop();
                var behavior = plugin.HasInstance 
                    ? plugin.Instance
                    : _container.GetInstance<IBehavior>(plugin.Type,
                         Dependency.For<IBehaviorChain>(this));

                if (behavior.ShouldRun()) return await behavior.Invoke();
            }
        }
    }
}