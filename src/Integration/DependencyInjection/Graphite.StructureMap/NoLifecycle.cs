using System;
using StructureMap;
using StructureMap.Pipeline;

namespace Graphite.StructureMap
{
    public class NoLifecycle : LifecycleBase, IAppropriateForNestedContainer, IObjectCache
    {
        public int Count { get; } = 0;

        public override IObjectCache FindCache(ILifecycleContext context)
        {
            return this;
        }

        public object Get(Type pluginType, Instance instance, IBuildSession session)
        {
            var lightweigtInstance = instance as LightweightObjectInstance;
            if (lightweigtInstance == null)
                throw new InvalidOperationException("No lifecycle " +
                    "only works with lightweight object instances.");
            return lightweigtInstance.Object;
        }

        public bool Has(Type pluginType, Instance instance)
        {
            return false;
        }

        public override void EjectAll(ILifecycleContext context) { }
        public void Eject(Type pluginType, Instance instance) { }
        public void DisposeAndClear() { }
    }
}