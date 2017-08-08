using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Graphite.Extensions;
using Tests.Common;
using Graphite.Reflection;
using NUnit.Framework;
using Should;

namespace Tests.Unit.Reflection
{
    [TestFixture]
    public class DescriptorTests
    {
        [Test]
        public void Should_return_basic_type_info()
        {
            var type = new TypeCache().GetTypeDescriptor(typeof(List<string>));

            type.Type.ShouldEqual<List<string>>();
            type.FriendlyName.ShouldEqual("List<string>");
            type.FriendlyFullName.ShouldEqual("System.Collections.Generic.List<string>");
        }

        [TestCase(typeof(string), true)]
        [TestCase(typeof(DescriptorTests), false)]
        [TestCase(typeof(Task), true)]
        [TestCase(typeof(Task<DescriptorTests>), false)]
        [TestCase(typeof(Task<string>), true)]
        public void Should_indicate_if_type_is_a_bcl_type(Type type, bool bclClass)
        {
            new TypeCache().GetTypeDescriptor(type)
                .IsBclType.ShouldEqual(bclClass);
        }

        [TestCase(typeof(string), true)]
        [TestCase(typeof(DescriptorTests), false)]
        [TestCase(typeof(Task), false)]
        [TestCase(typeof(Task<DescriptorTests>), false)]
        [TestCase(typeof(Task<string>), true)]
        public void Should_indicate_if_type_is_a_simple_type(Type type, bool bclClass)
        {
            new TypeCache().GetTypeDescriptor(type)
                .IsSimpleType.ShouldEqual(bclClass);
        }

        public class TypeMethods
        {
            public void Method1() { }
            public void Method2() { }
        }

        [Test]
        public void Should_enumerate_methods()
        {
            var methods = new TypeCache().GetTypeDescriptor(typeof(TypeMethods)).Methods;

            methods.Length.ShouldBeGreaterThanOrEqualTo(2);

            methods.ShouldContain(x => x.MethodInfo == Type<TypeMethods>.Method(t => t.Method1()));
            methods.ShouldContain(x => x.MethodInfo == Type<TypeMethods>.Method(t => t.Method2()));
        }

        public class TypeProperties
        {
            public string Property1 { get; set; }
            public string Property2 { get; set; }
        }

        [Test]
        public void Should_enumerate_properties()
        {
            var properties = new TypeCache().GetTypeDescriptor(typeof(TypeProperties)).Properties;

            properties.Length.ShouldBeGreaterThanOrEqualTo(2);

            properties.ShouldContain(x => x.PropertyInfo == Type<TypeProperties>.Property(t => t.Property1));
            properties.ShouldContain(x => x.PropertyInfo == Type<TypeProperties>.Property(t => t.Property2));
        }

        [TestCase(typeof(int?), true)]
        [TestCase(typeof(int), false)]
        [TestCase(typeof(Task), false)]
        [TestCase(typeof(Task<int>), false)]
        [TestCase(typeof(Task<int?>), true)]
        public void Should_indicate_if_type_is_nullable(Type type, bool nullable)
        {
            new TypeCache().GetTypeDescriptor(type)
                .IsNullable.ShouldEqual(nullable);
        }

        [TestCase(typeof(int?), typeof(int))]
        [TestCase(typeof(int), typeof(int))]
        [TestCase(typeof(Task), typeof(void))]
        [TestCase(typeof(Task<DescriptorTests>), typeof(DescriptorTests))]
        [TestCase(typeof(Task<int>), typeof(int))]
        [TestCase(typeof(Task<int?>), typeof(int))]
        public void Should_get_underlying_nullable_type(Type type, Type underlyingType)
        {
            new TypeCache().GetTypeDescriptor(type)
                .UnderlyingNullableType.Type.ShouldEqual(underlyingType);
        }

        [TestCase(typeof(int[]), true)]
        [TestCase(typeof(int), false)]
        [TestCase(typeof(Task), false)]
        [TestCase(typeof(Task<int>), false)]
        [TestCase(typeof(Task<int[]>), true)]
        public void Should_indicate_if_type_is_an_array(Type type, bool array)
        {
            new TypeCache().GetTypeDescriptor(type)
                .IsArray.ShouldEqual(array);
        }

        [TestCase(typeof(int), null)]
        [TestCase(typeof(int[]), typeof(int))]
        [TestCase(typeof(Task), null)]
        [TestCase(typeof(Task<DescriptorTests>), null)]
        [TestCase(typeof(Task<int>), null)]
        [TestCase(typeof(Task<int[]>), typeof(int))]
        public void Should_get_array_element_type(Type type, Type elementType)
        {
            (new TypeCache().GetTypeDescriptor(type)
                .ArrayElementType?.Type).ShouldEqual(elementType);
        }

        [TestCase(typeof(int), false)]
        [TestCase(typeof(List<int>), true)]
        [TestCase(typeof(IList<int>), true)]
        [TestCase(typeof(IEnumerable<int>), true)]
        [TestCase(typeof(ICollection<int>), true)]
        [TestCase(typeof(Task), false)]
        [TestCase(typeof(Task<int>), false)]
        [TestCase(typeof(Task<List<int>>), true)]
        [TestCase(typeof(Task<IList<int>>), true)]
        [TestCase(typeof(Task<IEnumerable<int>>), true)]
        [TestCase(typeof(Task<ICollection<int>>), true)]
        public void Should_indicate_if_type_is_generic_list_castable(Type type, bool listCastable)
        {
            new TypeCache().GetTypeDescriptor(type)
                .IsGenericListCastable.ShouldEqual(listCastable);
        }

        public class InheritedType : List<int> { }

        [TestCase(typeof(int), null)]
        [TestCase(typeof(List<int>), typeof(int))]
        [TestCase(typeof(IList<int>), typeof(int))]
        [TestCase(typeof(IEnumerable<int>), typeof(int))]
        [TestCase(typeof(ICollection<int>), typeof(int))]
        [TestCase(typeof(Task), null)]
        [TestCase(typeof(Task<DescriptorTests>), null)]
        [TestCase(typeof(Task<List<int>>), typeof(int))]
        [TestCase(typeof(Task<IList<int>>), typeof(int))]
        [TestCase(typeof(Task<IEnumerable<int>>), typeof(int))]
        [TestCase(typeof(Task<ICollection<int>>), typeof(int))]
        public void Should_get_generic_list_castable_element_type(Type type, Type elementType)
        {
            (new TypeCache().GetTypeDescriptor(type)
                .GenericListCastableElementType?.Type).ShouldEqual(elementType);
        }

        [TestCase(typeof(int), null)]
        [TestCase(typeof(int[]), typeof(int))]
        [TestCase(typeof(List<int>), typeof(int))]
        [TestCase(typeof(IList<int>), typeof(int))]
        [TestCase(typeof(IEnumerable<int>), typeof(int))]
        [TestCase(typeof(ICollection<int>), typeof(int))]
        public void Should_get_element_type(Type type, Type elementType)
        {
            (new TypeCache().GetTypeDescriptor(type)
                .ElementType?.Type).ShouldEqual(elementType);
        }

        [TestCase(typeof(int), false)]
        [TestCase(typeof(Task<int>), false)]
        [TestCase(typeof(Task), true)]
        public void Should_indicate_if_type_is_a_task(Type type, bool task)
        {
            new TypeCache().GetTypeDescriptor(type)
                .IsTask.ShouldEqual(task);
        }

        [TestCase(typeof(int), false)]
        [TestCase(typeof(Task), false)]
        [TestCase(typeof(Task<int>), true)]
        public void Should_indicate_if_type_is_a_task_with_a_result(Type type, bool task)
        {
            new TypeCache().GetTypeDescriptor(type)
                .IsTaskWithResult.ShouldEqual(task);
        }

        [TestCase(typeof(int), typeof(int))]
        [TestCase(typeof(Task), typeof(void))]
        [TestCase(typeof(Task<int>), typeof(int))]
        public void Should_return_task_result_type(Type type, Type taskResultType)
        {
            (new TypeCache().GetTypeDescriptor(type).Type).ShouldEqual(taskResultType);
        }
    }
}
