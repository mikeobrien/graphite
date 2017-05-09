using System;
using System.Collections.Generic;

namespace Graphite.DependencyInjection
{
    public interface IContainer : IDisposable
    {
        IContainer CreateScopedContainer();
        object GetInstance(Type type, params Dependency[] dependencies);
        IEnumerable<object> GetInstances(Type type);
        void Register(Type plugin, Type concrete, bool singleton);
        void Register(Type plugin, object instance, bool dispose);
    }
}
