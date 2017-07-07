using System;
using System.Collections.Generic;
using System.Web.Http;
using Graphite.Reflection;
using NUnit.Framework;
using Should;
using Tests.Common;
using Tests.Unit.Reflection.ParentNamespace;
using Tests.Unit.Reflection.ParentNamespace.ChildNamespace1;

namespace Tests.Unit.Reflection
{
    public class NotUnderNamespaceClass { }
    namespace ParentNamespace
    {
        public class MarkerClass { }
        public class InNamespaceClass { }
        namespace ChildNamespace1
        {
            public class UnderNamespaceClass { }
        }
        namespace ChildNamespace2
        {
            public class UnderNamespaceClass { }
        }
    }

    [TestFixture]
    public class ExtensionTests
    {
        public class EmptyCtor { }
        public class NoEmptyCtor { public NoEmptyCtor(string fark) { } }

        [TestCase(typeof(EmptyCtor), true)]
        [TestCase(typeof(NoEmptyCtor), false)]
        public void Should_compile_try_create_function(Type type, bool canCreate)
        {
            var result = type.CompileTryCreate()();

            if (canCreate) result.ShouldNotBeNull();
            else result.ShouldBeNull();
        }

        [Test, PerformanceTest]
        public void Should_be_faster_than_activator_create_object()
        {
            var comparison = PerformanceComparison.InTicks(10000, 1000);
            var type = typeof(EmptyCtor);
            var create = type.CompileTryCreate();

            comparison.AddCase("Native", () => new EmptyCtor());
            var expressionCase = comparison.AddCase("Compiled expression", () => create());
            var reflectionCase = comparison.AddCase("Reflection", () => Activator.CreateInstance(type));

            comparison.Run();

            expressionCase.Average.ShouldBeLessThan(reflectionCase.Average);
        }

        public class TypeWithProperty
        {
            public string Value { get; set; }
        }

        [Test]
        public void Should_set_property_value()
        {
            var instance = new TypeWithProperty();

            typeof(TypeWithProperty).GetProperty(nameof(TypeWithProperty.Value))
                .CompileSetter()(instance, "fark");

            instance.Value.ShouldEqual("fark");
        }

        [Test, PerformanceTest]
        public void Should_be_faster_than_reflection_setting_property_value()
        {
            var comparison = PerformanceComparison.InTicks(10000, 1000);
            var instance = new TypeWithProperty();
            var propertyInfo = typeof(TypeWithProperty).GetProperty(
                nameof(TypeWithProperty.Value));
            var setter = propertyInfo.CompileSetter();

            comparison.AddCase("Native", () => instance.Value = "fark");
            var expressionCase = comparison.AddCase("Compiled expression", () => setter(instance, "fark"));
            var reflectionCase = comparison.AddCase("Reflection", () => propertyInfo.SetValue(instance, "fark"));

            comparison.Run();

            expressionCase.Average.ShouldBeLessThan(reflectionCase.Average);
        }

        [Test]
        public void Should_get_property_value()
        {
            var instance = new TypeWithProperty
            {
                Value = "fark"
            };

            typeof(TypeWithProperty).GetProperty(nameof(TypeWithProperty.Value))
                .CompileGetter()(instance)
                .ShouldEqual("fark");
        }

        [Test, PerformanceTest]
        public void Should_be_faster_than_reflection_getting_property_value()
        {
            var comparison = PerformanceComparison.InTicks(10000, 1000);
            var instance = new TypeWithProperty
            {
                Value = "fark"
            };
            var propertyInfo = typeof(TypeWithProperty).GetProperty(
                nameof(TypeWithProperty.Value));
            var getter = propertyInfo.CompileGetter();
            object value = null;
            
            comparison.AddCase("Native", () => value = instance.Value);
            var expressionCase = comparison.AddCase("Compiled expression", () => getter(instance));
            var reflectionCase = comparison.AddCase("Reflection", () => propertyInfo.GetValue(instance));

            comparison.Run();

            expressionCase.Average.ShouldBeLessThan(reflectionCase.Average);
        }

        [TestCase(typeof(Dictionary<string, KeyValuePair<int, string>>), "Dictionary")]
        [TestCase(typeof(string), "String")]
        [TestCase(typeof(int?), "Nullable")]
        public void should_get_generic_type_base_full_name(Type type, string name)
        {
            type.GetNonGenericName().ShouldEqual(name);
        }

        public class NestedType { }

        [TestCase(typeof(NestedType), false, "NestedType")]
        [TestCase(typeof(Dictionary<string, KeyValuePair<int, string>>), false,
            "Dictionary<string, KeyValuePair<int, string>>")]
        [TestCase(typeof(string), false, "string")]
        [TestCase(typeof(KeyValuePair<int?, string>?), false, "KeyValuePair<int?, string>?")]

        [TestCase(typeof(NestedType), true, "Tests.Unit.Reflection.ExtensionTests.NestedType")]
        [TestCase(typeof(Dictionary<string, KeyValuePair<int, string>>), true,
            "System.Collections.Generic.Dictionary<string, System.Collections" +
            ".Generic.KeyValuePair<int, string>>")]
        [TestCase(typeof(string), true, "string")]
        [TestCase(typeof(KeyValuePair<int?, string>?), true, "System." +
            "Collections.Generic.KeyValuePair<int?, string>?")]
        public void should_get_friendly_full_type_name(Type type, bool includeNamespace, string name)
        {
            type.GetFriendlyTypeName(includeNamespace).ShouldEqual(name);
        }

        [Test]
        public void Should_get_nested_type_name()
        {
            typeof(NestedType).GetNestedName()
                .ShouldEqual($"{nameof(ExtensionTests)}+{nameof(NestedType)}");
        }

        public class MethodNames
        {
            public void Method() { }
            public void GenericMethod<T1, T2, T3>() { }
        }

        public void should_get_friendly_method_name([Values(true, false)] bool includeNamespace)
        {
            typeof(MethodNames).GetMethod(nameof(MethodNames.Method))
                .GetFriendlyMethodName(includeNamespace)
                .ShouldEqual(nameof(MethodNames.Method));
        }

        [TestCase(false, "GenericMethod<Dictionary<string, KeyValuePair<int, string>>, string, KeyValuePair<int?, string>?>")]
        [TestCase(true, "GenericMethod<System.Collections.Generic" +
            ".Dictionary<string, System.Collections.Generic" +
            ".KeyValuePair<int, string>>, string, System" +
            ".Collections.Generic.KeyValuePair<int?, string>?>")]
        public void should_get_friendly_generic_method_name(bool includeNamespace, string name)
        {
            var method = typeof(MethodNames).GetMethod(nameof(MethodNames.GenericMethod))
                .MakeGenericMethod(typeof(Dictionary<string, KeyValuePair<int, string>>), 
                    typeof(string), typeof(KeyValuePair<int?, string>?));
            
            method.GetFriendlyMethodName(includeNamespace).ShouldEqual(name);
        }

        [TestCase(typeof(string), true)]
        [TestCase(typeof(GlobalConfiguration), true)]
        [TestCase(typeof(ExtensionTests), false)]
        public void Should_indicate_if_assembly_contains_system_namespace(Type type, bool system)
        {
            type.Assembly.IsSystemAssembly().ShouldEqual(system);
        }

        [TestCase(typeof(NotUnderNamespaceClass), false)]
        [TestCase(typeof(InNamespaceClass), true)]
        [TestCase(typeof(UnderNamespaceClass), true)]
        public void Should_indicate_if_type_is_under_namespace(Type type, bool under)
        {
            type.IsUnderNamespace<MarkerClass>().ShouldEqual(under); ;
        }

        [TestCase("ChildNamespace1", true)]
        [TestCase("ChildNamespace2", false)]
        public void Should_indicate_if_type_is_under_relative_namespace(string relativeNamespace, bool under)
        {
            typeof(UnderNamespaceClass).IsUnderNamespace<MarkerClass>(relativeNamespace).ShouldEqual(under);
        }

        [TestCase(typeof(int), typeof(int))]
        [TestCase(typeof(int?), typeof(int))]
        public void should_get_underlying_nullable_type(Type source, Type expected)
        {
            source.GetUnderlyingNullableType().ShouldEqual(expected);
        }

        [Test]
        public void should_indicate_if_a_type_is_nullable()
        {
            typeof(int).IsNullable().ShouldBeFalse();
            typeof(int?).IsNullable().ShouldBeTrue();
        }

        [TestCase(typeof(string), null)]
        [TestCase(typeof(byte[]), typeof(byte))]
        [TestCase(typeof(byte?[]), typeof(byte?))]
        public void Should_get_array_element_type(Type source, Type expected)
        {
            source.TryGetArrayElementType().ShouldEqual(expected);
        }

        [TestCase(typeof(string), null)]
        [TestCase(typeof(IObserver<int>), null)]
        [TestCase(typeof(List<string>), typeof(string))]
        [TestCase(typeof(IList<string>), typeof(string))]
        [TestCase(typeof(IEnumerable<string>), typeof(string))]
        [TestCase(typeof(ICollection<string>), typeof(string))]
        public void Should_get_generic_list_castable_element_type(Type source, Type expected)
        {
            source.TryGetGenericListCastableElementType().ShouldEqual(expected);
        }

        [Test]
        [TestCase(typeof(string))]
        [TestCase(typeof(Uri))]
        [TestCase(typeof(ConsoleColor)), TestCase(typeof(ConsoleColor?))]
        [TestCase(typeof(char)), TestCase(typeof(char?))]
        [TestCase(typeof(decimal)), TestCase(typeof(decimal?))]
        [TestCase(typeof(bool)), TestCase(typeof(bool?))]
        [TestCase(typeof(byte)), TestCase(typeof(byte?))]
        [TestCase(typeof(sbyte)), TestCase(typeof(sbyte?))]
        [TestCase(typeof(short)), TestCase(typeof(short?))]
        [TestCase(typeof(ushort)), TestCase(typeof(ushort?))]
        [TestCase(typeof(int)), TestCase(typeof(int?))]
        [TestCase(typeof(uint)), TestCase(typeof(uint?))]
        [TestCase(typeof(long)), TestCase(typeof(long?))]
        [TestCase(typeof(ulong)), TestCase(typeof(ulong?))]
        [TestCase(typeof(double)), TestCase(typeof(double?))]
        [TestCase(typeof(float)), TestCase(typeof(float?))]
        [TestCase(typeof(IntPtr)), TestCase(typeof(IntPtr?))]
        [TestCase(typeof(UIntPtr)), TestCase(typeof(UIntPtr?))]
        [TestCase(typeof(DateTime)), TestCase(typeof(DateTime?))]
        [TestCase(typeof(TimeSpan)), TestCase(typeof(TimeSpan?))]
        [TestCase(typeof(Guid)), TestCase(typeof(Guid?))]
        public void should_indicate_if_a_type_is_simple(Type type)
        {
            type.IsSimpleType().ShouldBeTrue();
        }

        [Test]
        [TestCase(typeof(KeyValuePair<string, int>))]
        [TestCase(typeof(Tuple))]
        [TestCase(typeof(object))]
        public void should_indicate_if_a_type_is_not_simple(Type type)
        {
            type.IsSimpleType().ShouldBeFalse();
        }

        public static readonly object[] SimpleTypeParsing = TestCaseSource.Create(x => x
            .Add<string>("1")
            .Add<Uri>(new Uri("http://www.xkcd.com"))

            .Add<UriFormat>(UriFormat.UriEscaped).Add<UriFormat?>(UriFormat.UriEscaped)

            .Add<DateTime>(DateTime.Today).Add<DateTime?>(DateTime.Today)
            .Add<TimeSpan>(TimeSpan.MaxValue).Add<TimeSpan?>(TimeSpan.MaxValue)
            .Add<Guid>(Guid.Empty).Add<Guid?>(Guid.Empty)

            .Add<Boolean>(true).Add<Boolean?>(true)
            .Add<Byte>(5).Add<Byte?>(55)
            .Add<SByte>(6).Add<SByte?>(66)
            .Add<Int16>(7).Add<Int16?>(77)
            .Add<UInt16>(8).Add<UInt16?>(88)
            .Add<Int32>(9).Add<Int32?>(99)
            .Add<UInt32>(10).Add<UInt32?>(110)
            .Add<Int64>(11).Add<Int64?>(111)
            .Add<UInt64>(12).Add<UInt64?>(120)
            .Add<IntPtr>(new IntPtr(13)).Add<IntPtr?>(new IntPtr(130))
            .Add<UIntPtr>(new UIntPtr(14)).Add<UIntPtr?>(new UIntPtr(140))
            .Add<Char>('a').Add<Char?>('b')
            .Add<Double>(15).Add<Double?>(150)
            .Add<Single>(16).Add<Single?>(160)
            .Add<Decimal>(17).Add<Decimal?>(170));

        [Test]
        [TestCaseSource(nameof(SimpleTypeParsing))]
        public void should_parse_simple_types(Type type, object value)
        {
            var result = value.ToString().ParseSimpleType(
                new TypeCache().GetTypeDescriptor(type));
            result.ShouldEqual(value);
            result.ShouldBeType(type.GetUnderlyingNullableType());
        }

        [Test]
        public void should_parse_enum_integer()
        {
            "2".ParseSimpleType(new TypeCache().GetTypeDescriptor(typeof(UriFormat)))
                .ShouldEqual(UriFormat.Unescaped);
        }

        [Test]
        public void should_parse_enum_case_insensitively()
        {
            "UNESCAPED".ParseSimpleType(new TypeCache().GetTypeDescriptor(typeof(UriFormat)))
                .ShouldEqual(UriFormat.Unescaped);
        }

        [Test]
        public void should_parse_bool_case_insensitively()
        {
            "TRUE".ParseSimpleType(new TypeCache().GetTypeDescriptor(typeof(bool)))
                .ShouldEqual(true);
        }

        [Test]
        public void should_return_empty_string_when_source_is_empty_string()
        {
            "".ParseSimpleType(new TypeCache().GetTypeDescriptor(
                typeof(string))).ShouldEqual("");
        }

        [Test]
        [TestCase(typeof(string))]
        [TestCase(typeof(Uri))]
        [TestCase(typeof(UriFormat)), TestCase(typeof(UriFormat?))]
        [TestCase(typeof(DateTime)), TestCase(typeof(DateTime?))]
        [TestCase(typeof(TimeSpan)), TestCase(typeof(TimeSpan?))]
        [TestCase(typeof(Guid)), TestCase(typeof(Guid?))]
        [TestCase(typeof(Boolean)), TestCase(typeof(Boolean?))]
        [TestCase(typeof(Byte)), TestCase(typeof(Byte?))]
        [TestCase(typeof(SByte)), TestCase(typeof(SByte?))]
        [TestCase(typeof(Int16)), TestCase(typeof(Int16?))]
        [TestCase(typeof(UInt16)), TestCase(typeof(UInt16?))]
        [TestCase(typeof(Int32)), TestCase(typeof(Int32?))]
        [TestCase(typeof(UInt32)), TestCase(typeof(UInt32?))]
        [TestCase(typeof(Int64)), TestCase(typeof(Int64?))]
        [TestCase(typeof(UInt64)), TestCase(typeof(UInt64?))]
        [TestCase(typeof(IntPtr)), TestCase(typeof(IntPtr?))]
        [TestCase(typeof(UIntPtr)), TestCase(typeof(UIntPtr?))]
        [TestCase(typeof(Char)), TestCase(typeof(Char?))]
        [TestCase(typeof(Double)), TestCase(typeof(Double?))]
        [TestCase(typeof(Single)), TestCase(typeof(Single?))]
        [TestCase(typeof(Decimal)), TestCase(typeof(Decimal?))]
        public void should_return_null_when_source_is_null(Type type)
        {
            ((string)null).ParseSimpleType(new TypeCache()
                .GetTypeDescriptor(type)).ShouldEqual(null);
        }

        [Test]
        [TestCase(typeof(Uri))]
        [TestCase(typeof(UriFormat)), TestCase(typeof(UriFormat?))]
        [TestCase(typeof(DateTime)), TestCase(typeof(DateTime?))]
        [TestCase(typeof(TimeSpan)), TestCase(typeof(TimeSpan?))]
        [TestCase(typeof(Guid)), TestCase(typeof(Guid?))]
        [TestCase(typeof(Boolean)), TestCase(typeof(Boolean?))]
        [TestCase(typeof(Byte)), TestCase(typeof(Byte?))]
        [TestCase(typeof(SByte)), TestCase(typeof(SByte?))]
        [TestCase(typeof(Int16)), TestCase(typeof(Int16?))]
        [TestCase(typeof(UInt16)), TestCase(typeof(UInt16?))]
        [TestCase(typeof(Int32)), TestCase(typeof(Int32?))]
        [TestCase(typeof(UInt32)), TestCase(typeof(UInt32?))]
        [TestCase(typeof(Int64)), TestCase(typeof(Int64?))]
        [TestCase(typeof(UInt64)), TestCase(typeof(UInt64?))]
        [TestCase(typeof(IntPtr)), TestCase(typeof(IntPtr?))]
        [TestCase(typeof(UIntPtr)), TestCase(typeof(UIntPtr?))]
        [TestCase(typeof(Char)), TestCase(typeof(Char?))]
        [TestCase(typeof(Double)), TestCase(typeof(Double?))]
        [TestCase(typeof(Single)), TestCase(typeof(Single?))]
        [TestCase(typeof(Decimal)), TestCase(typeof(Decimal?))]
        public void should_fail_when_source_is_empty_string(Type type)
        {
            "".Should().Throw<FormatException>(x => x.ParseSimpleType(
                new TypeCache().GetTypeDescriptor(type)));
        }

        [Test]
        [TestCase(typeof(Uri))]
        [TestCase(typeof(UriFormat)), TestCase(typeof(UriFormat?))]
        [TestCase(typeof(DateTime)), TestCase(typeof(DateTime?))]
        [TestCase(typeof(TimeSpan)), TestCase(typeof(TimeSpan?))]
        [TestCase(typeof(Guid)), TestCase(typeof(Guid?))]
        [TestCase(typeof(Boolean)), TestCase(typeof(Boolean?))]
        [TestCase(typeof(Byte)), TestCase(typeof(Byte?))]
        [TestCase(typeof(SByte)), TestCase(typeof(SByte?))]
        [TestCase(typeof(Int16)), TestCase(typeof(Int16?))]
        [TestCase(typeof(UInt16)), TestCase(typeof(UInt16?))]
        [TestCase(typeof(Int32)), TestCase(typeof(Int32?))]
        [TestCase(typeof(UInt32)), TestCase(typeof(UInt32?))]
        [TestCase(typeof(Int64)), TestCase(typeof(Int64?))]
        [TestCase(typeof(UInt64)), TestCase(typeof(UInt64?))]
        [TestCase(typeof(IntPtr)), TestCase(typeof(IntPtr?))]
        [TestCase(typeof(UIntPtr)), TestCase(typeof(UIntPtr?))]
        [TestCase(typeof(Char)), TestCase(typeof(Char?))]
        [TestCase(typeof(Double)), TestCase(typeof(Double?))]
        [TestCase(typeof(Single)), TestCase(typeof(Single?))]
        [TestCase(typeof(Decimal)), TestCase(typeof(Decimal?))]
        public void should_fail_when_source_is_invalid_string(Type type)
        {
            "yada".Should().Throw<FormatException>(x => x.ParseSimpleType(
               new TypeCache().GetTypeDescriptor(type)));
        }
    }
}
