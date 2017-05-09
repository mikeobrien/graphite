using System;
using System.Linq;
using System.Web.Http.Dependencies;   
using Graphite.DependencyInjection;
using Graphite.Extensions;
using Graphite.StructureMap;
using NUnit.Framework;
using Should;

namespace Tests.Unit.DependencyInjection
{
    [TestFixture]
    public class StructureMapTests
    {
        public interface IPlugin : IDisposable
        {
            bool Disposed { get; }
        }

        public class Concrete : IPlugin
        {
            public bool Disposed { get; private set; }

            public void Dispose()
            {
                Console.WriteLine(Environment.StackTrace);
                Disposed = true;
            }
        }

        [Test]
        public void Should_create_scoped_container()
        {
            var container = new Container();
            container.Register<IPlugin>(typeof(Concrete), false);
            var scopedContainer = container.CreateScopedContainer();
            var instance = scopedContainer.GetInstance<IPlugin>();

            instance.ShouldNotBeNull();
            instance.ShouldBeType<Concrete>();

            instance.ShouldNotEqual(container.GetInstance<IPlugin>());

            instance.ShouldEqual(scopedContainer.GetInstance<IPlugin>());

            scopedContainer.Dispose();

            instance.Disposed.ShouldBeTrue();
        }

        [Test]
        public void Should_auto_resolve()
        {
            var instance = new Container().GetInstance<Concrete>();

            instance.ShouldNotBeNull();
            instance.ShouldBeType<Concrete>();
        }

        public class ImplicitDependency { }
        public interface IExplicitDependency { }
        public class ExplicitDependency : IExplicitDependency { }

        public class PluginWithDependencies
        {
            public PluginWithDependencies(ImplicitDependency implicitDependency, 
                IExplicitDependency explicitDependency)
            {
                ImplicitDependency = implicitDependency;
                ExplicitDependency = explicitDependency;
            }

            public ImplicitDependency ImplicitDependency { get; }
            public IExplicitDependency ExplicitDependency { get; }
        }

        [Test]
        public void Should_get_instance_with_explicit_dependencies()
        {
            var explicitDependency = new ExplicitDependency();
            var plugin = new Container().GetInstance<PluginWithDependencies>(
                new Dependency(typeof(IExplicitDependency), explicitDependency));

            plugin.ImplicitDependency.ShouldNotBeNull();
            plugin.ExplicitDependency.ShouldEqual(explicitDependency);
        }

        public interface IMultipleRegistrations { }
        public class Registration1 : IMultipleRegistrations { }
        public class Registration2 : IMultipleRegistrations { }

        [Test]
        public void Should_return_multiple_instances()
        {
            var container = new Container();
            container.Register<IMultipleRegistrations>(typeof(Registration1), false);
            container.Register<IMultipleRegistrations>(typeof(Registration2), false);

            var instances = container.GetInstances<IMultipleRegistrations>().ToList();

            instances.Count.ShouldEqual(2);
            instances.OfType<Registration1>().Count().ShouldEqual(1);
            instances.OfType<Registration2>().Count().ShouldEqual(1);
        }

        [Test]
        public void Should_register_transient_type()
        {
            var container = new Container();
            container.Register<IPlugin>(typeof(Concrete), false);
            var instance = container.GetInstance<IPlugin>();

            instance.ShouldNotBeNull();
            instance.ShouldBeType<Concrete>();

            container.GetInstance<IPlugin>().ShouldNotEqual(instance);
        }

        [Test]
        public void Should_register_singleton_type()
        {
            var container = new Container();
            container.Register<IPlugin>(typeof(Concrete), true);
            var instance = container.GetInstance<IPlugin>();

            instance.ShouldNotBeNull();
            instance.ShouldBeType<Concrete>();

            container.GetInstance<IPlugin>().ShouldEqual(instance);

            var scopedContainer = container.CreateScopedContainer();

            scopedContainer.GetInstance<IPlugin>().ShouldEqual(instance);
        }

        [Test]
        public void Should_register_instance()
        {
            var instance = new Concrete();
            var container = new Container();

            container.Register<IPlugin>(instance);

            container.GetInstance<IPlugin>()
                .ShouldEqual(instance);
        }

        [Test]
        public void Should_not_dispose_instances_when_configured([Values(true, false)] bool dispose)
        {
            var instance = new Concrete();
            var container = new Container();
            container.Register(instance, dispose);

            container.Dispose();

            instance.Disposed.ShouldEqual(dispose);
        }

        [Test]
        public void Should_not_dispose_instances_in_nested_container_when_configured([Values(true, false)] bool dispose)
        {
            var instance = new Concrete();
            var container = new Container();
            var nestedContainer = container.CreateScopedContainer();
            nestedContainer.Register(instance, dispose);

            // Force caching
            nestedContainer.GetInstance<Concrete>().ShouldEqual(instance);

            nestedContainer.Dispose();

            instance.Disposed.ShouldEqual(dispose);
        }

        /* ------------------ Dependency Resolver ------------------- */

        [Test]
        public void Should_create_scoped_container_from_dependency_resolver()
        {
            var container = new Container();
            var dependencyResolver = (IDependencyResolver)container;
            container.Register<IPlugin>(typeof(Concrete), false);

            var scopedContainer = dependencyResolver.BeginScope();
            var instance = scopedContainer.GetService(typeof(IPlugin));

            instance.ShouldNotBeNull();
            instance.ShouldBeType<Concrete>();

            instance.ShouldNotEqual(dependencyResolver.GetService(typeof(IPlugin)));

            instance.ShouldEqual(scopedContainer.GetService(typeof(IPlugin)));

            scopedContainer.Dispose();

            instance.CastTo<IPlugin>().Disposed.ShouldBeTrue();
        }

        [Test]
        public void Should_get_service_with_dependency_resolver()
        {
            var container = new Container();
            var dependencyResolver = (IDependencyResolver)container;
            container.Register<IPlugin>(typeof(Concrete), false);

            var instance = dependencyResolver.GetService(typeof(IPlugin));
            
            instance.ShouldNotBeNull();
            instance.ShouldBeType<Concrete>();
        }

        [Test]
        public void Should_return_multiple_instances_from_dependency_resolver()
        {
            var container = new Container();
            container.Register<IMultipleRegistrations>(typeof(Registration1), false);
            container.Register<IMultipleRegistrations>(typeof(Registration2), false);

            var instances = container.CastTo<IDependencyResolver>()
                .GetServices(typeof(IMultipleRegistrations)).ToList();

            instances.Count.ShouldEqual(2);
            instances.OfType<Registration1>().Count().ShouldEqual(1);
            instances.OfType<Registration2>().Count().ShouldEqual(1);
        }
    }
}
