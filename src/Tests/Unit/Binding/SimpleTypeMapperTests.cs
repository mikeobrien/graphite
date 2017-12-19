using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Graphite;
using Graphite.Actions;
using Graphite.Binding;
using Graphite.Extensions;
using Graphite.Linq;
using Graphite.Reflection;
using Graphite.Routing;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Binding
{
    [TestFixture]
    public class SimpleTypeMapperTests
    {
        public class Handler { public void Post() { } }

        private ActionMethod _actionMethod;
        private Configuration _configuration;

        [SetUp]
        public void Setup()
        {
            _configuration = new Configuration();
            _actionMethod = ActionMethod.From<Handler>(x => x.Post());
        }

        public void SimpleParameterTypes(string stringValue,
            AttributeTargets enumValue, char charValue, bool boolValue, sbyte sbyteValue,
            byte byteValue, short shortValue, ushort ushortValue, int intValue, uint uintValue,
            long longValue, ulong ulongValue, float floatValue, double doubleValue, decimal decimalValue,
            DateTime datetimeValue, Guid guidValue, TimeSpan tiemspanValue, Uri uriValue,

            AttributeTargets? enumNullableValue, char? charNullableValue, bool? boolNullableValue,
            sbyte? sbyteNullableValue, byte? byteNullableValue, short? shortNullableValue,
            ushort? ushortNullableValue, int? intNullableValue, uint? uintNullableValue,
            long? longNullableValue, ulong? ulongNullableValue, float? floatNullableValue,
            double? doubleNullableValue, decimal? decimalNullableValue, DateTime? datetimeNullableValue,
            Guid? guidNullableValue, TimeSpan? timespanNullableValue) { }

        private static readonly ParameterDescriptor[] SimpleTypeParameters =
            new TypeCache().GetTypeDescriptor(typeof(SimpleTypeMapperTests)).Methods
                .First(x => x.Name == nameof(SimpleParameterTypes)).Parameters;

        public static object[][] SimpleTypeParameterTestCases = SimpleTypeParameters
            .Select(x => new object[] { x }).ToArray();

        [TestCaseSource(nameof(SimpleTypeParameterTestCases))]
        public void Should_apply_to_simple_types(ParameterDescriptor parameter)
        {
            new SimpleTypeMapper(new ParsedValueMapper()).AppliesTo(new ValueMapperContext(
                new ActionParameter(_actionMethod, parameter), null)).ShouldBeTrue();
        }

        public class ComplexClass { }
        public struct ComplexStruct { }

        public void ComplexParameterTypes(ComplexClass complexClass, ComplexStruct complexStruct,
            ComplexStruct complexNullableStruct) { }

        public static object[][] ComplexParameterTypeTestCases = new TypeCache().GetTypeDescriptor(typeof(SimpleTypeMapperTests))
            .Methods.First(x => x.Name == nameof(ComplexParameterTypes)).Parameters.Select(x => new object[] { x }).ToArray();

        [TestCaseSource(nameof(ComplexParameterTypeTestCases))]
        public void Should_not_apply_to_complex_types(ParameterDescriptor parameter)
        {
            new SimpleTypeMapper(new ParsedValueMapper()).AppliesTo(new ValueMapperContext(
                new ActionParameter(_actionMethod, parameter), null)).ShouldBeFalse();
        }

        private static DateTime _datetime = DateTime.Parse(DateTime.MaxValue.ToString());

        public static object[][] SimpleTypeParameterParsingTestCases = TestCaseSource.Create<object>(x => x
            .Add<string>("string", "string")
            .Add<AttributeTargets>(AttributeTargets.ReturnValue, "ReturnValue")
            .Add<AttributeTargets>(AttributeTargets.ReturnValue, AttributeTargets.ReturnValue)
            .Add<AttributeTargets?>((AttributeTargets?)null, null)
            .Add<AttributeTargets?>((AttributeTargets?)AttributeTargets.ReturnValue, AttributeTargets.ReturnValue)
            .Add<char>('a', "a").Add<char>('a', 'a')
            .Add<char?>((char?)null, null).Add<char?>((char?)'a', 'a')
            .Add<bool>(true, "true").Add<bool>(true, true)
            .Add<bool?>((bool?)null, null).Add<bool?>((bool?)true, (bool?)true)
            .Add<sbyte>(1, "1").Add<sbyte>(1, (sbyte)1)
            .Add<sbyte?>((sbyte?)null, null).Add<sbyte?>((sbyte?)1, (sbyte?)1)
            .Add<byte>(2, "2").Add<byte>(2, (byte)2)
            .Add<byte?>((byte?)null, null).Add<byte?>((byte?)2, (byte?)2)
            .Add<short>(3, "3").Add<short>(3, (short)3)
            .Add<short?>((short?)null, null).Add<short?>((short?)3, (short?)3)
            .Add<ushort>(4, "4").Add<ushort>(4, (ushort)4)
            .Add<ushort?>((ushort?)null, null).Add<ushort?>((ushort?)4, (ushort?)4)
            .Add<int>(5, "5").Add<int>(5, (int)5)
            .Add<int?>((int?)null, null).Add<int?>((int?)5, (int?)5)
            .Add<uint>(6, "6").Add<uint>(6, (uint)6)
            .Add<uint?>((uint?)null, null).Add<uint?>((uint?)6, (uint?)6)
            .Add<long>(7, "7").Add<long>(7, (long)7)
            .Add<long?>((long?)null, null).Add<long?>((long?)7, (long?)7)
            .Add<ulong>(8, "8").Add<ulong>(8, (ulong)8)
            .Add<ulong?>((ulong?)null, null).Add<ulong?>((ulong?)8, (ulong?)8)
            .Add<float>(9, "9").Add<float>(9, (float)9)
            .Add<float?>((float?)null, null).Add<float?>((float?)9, (float?)9)
            .Add<double>(10, "10").Add<double>(10, (double)10)
            .Add<double?>((double?)null, null).Add<double?>((double?)10, (double?)10)
            .Add<decimal>(11, "11").Add<decimal>(11, (decimal)11)
            .Add<decimal?>((decimal?)null, null).Add<decimal?>((decimal?)11, (decimal?)11)
            .Add<DateTime>(_datetime, _datetime.ToString())
            .Add<DateTime>(DateTime.MaxValue, DateTime.MaxValue)
            .Add<DateTime?>((DateTime?)null, null)
            .Add<DateTime?>((DateTime?)DateTime.MaxValue, (DateTime?)DateTime.MaxValue)
            .Add<Guid>(Guid.Parse("ebfdcbd2-e8a3-40fa-8b08-67afe730ea14"), "ebfdcbd2-e8a3-40fa-8b08-67afe730ea14")
            .Add<Guid>(Guid.Parse("ebfdcbd2-e8a3-40fa-8b08-67afe730ea14"), Guid.Parse("ebfdcbd2-e8a3-40fa-8b08-67afe730ea14"))
            .Add<Guid?>((Guid?)null, null)
            .Add<Guid?>((Guid?)Guid.Parse("ebfdcbd2-e8a3-40fa-8b08-67afe730ea14"), (Guid?)Guid.Parse("ebfdcbd2-e8a3-40fa-8b08-67afe730ea14"))
            .Add<TimeSpan>(TimeSpan.MaxValue, TimeSpan.MaxValue.ToString())
            .Add<TimeSpan>(TimeSpan.MaxValue, TimeSpan.MaxValue)
            .Add<TimeSpan?>((TimeSpan?)null, null)
            .Add<TimeSpan?>((TimeSpan?)TimeSpan.MaxValue, (TimeSpan?)TimeSpan.MaxValue));

        [TestCaseSource(nameof(SimpleTypeParameterParsingTestCases))]
        public void Should_map_simple_types(Type type, object expected, object value)
        {
            var parameter = SimpleTypeParameters.FirstOrDefault(x => x.ParameterType.Type == type);
            var result = new SimpleTypeMapper(new ParsedValueMapper()).Map(new ValueMapperContext(
                new ActionParameter(_actionMethod, parameter), value.AsArray()));

            result.Status.ShouldEqual(MappingStatus.Success);
            result.Value?.GetType().ShouldEqual(type.GetUnderlyingNullableType());
            result.Value.ShouldEqual(expected);
        }

        [TestCaseSource(nameof(SimpleTypeParameterParsingTestCases))]
        public void Should_return_failure_when_format_incorrect(
            Type type, object expected, object value)
        {
            if (type == typeof(string)) return;
            var parameter = SimpleTypeParameters.FirstOrDefault(x => x.ParameterType.Type == type);
            var result = new SimpleTypeMapper(new ParsedValueMapper()).Map(new ValueMapperContext(
                new ActionParameter(_actionMethod, parameter), "fark".AsArray()));

            result.Status.ShouldEqual(MappingStatus.Failure);
            result.ErrorMessage.ShouldContain("fark");
            result.ErrorMessage.ShouldContain(parameter.Name);
        }

        private static readonly ParameterDescriptor[] SimpleTypeParameterArrays =
            new TypeCache().GetTypeDescriptor(typeof(SimpleTypeMapperTests)).Methods
                .First(x => x.Name == nameof(SimpleParameterTypeArrays)).Parameters;

        public void SimpleParameterTypeArrays(string[] stringValue,
            AttributeTargets[] enumValue, char[] charValue, bool[] boolValue, sbyte[] sbyteValue,
            byte[] byteValue, short[] shortValue, ushort[] ushortValue, int[] intValue, uint[] uintValue,
            long[] longValue, ulong[] ulongValue, float[] floatValue, double[] doubleValue, decimal[] decimalValue,
            DateTime[] datetimeValue, Guid[] guidValue, TimeSpan[] tiemspanValue, Uri[] uriValue,

            AttributeTargets?[] enumNullableValue, char?[] charNullableValue, bool?[] boolNullableValue,
            sbyte?[] sbyteNullableValue, byte?[] byteNullableValue, short?[] shortNullableValue,
            ushort?[] ushortNullableValue, int?[] intNullableValue, uint?[] uintNullableValue,
            long?[] longNullableValue, ulong?[] ulongNullableValue, float?[] floatNullableValue,
            double?[] doubleNullableValue, decimal?[] decimalNullableValue, DateTime?[] datetimeNullableValue,
            Guid?[] guidNullableValue, TimeSpan?[] timespanNullableValue) { }

        [TestCaseSource(nameof(SimpleTypeParameterParsingTestCases))]
        public void Should_map_simple_types_to_an_array(Type type, object expected, object value)
        {
            var parameter = SimpleTypeParameterArrays.FirstOrDefault(
                x => x.ParameterType.ElementType.Type == type);
            var result = new SimpleTypeMapper(new ParsedValueMapper()).Map(new ValueMapperContext(
                new ActionParameter(_actionMethod, parameter), new[] { value, value }));

            result.Status.ShouldEqual(MappingStatus.Success);
            result.Value?.GetType().ShouldEqual(type.MakeArrayType());
            result.Value.As<IEnumerable>().Cast<object>().ShouldOnlyContain(expected, expected);
        }

        private static readonly ParameterDescriptor[] SimpleTypeParameterLists =
            new TypeCache().GetTypeDescriptor(typeof(SimpleTypeMapperTests)).Methods
                .First(x => x.Name == nameof(SimpleParameterTypeLists)).Parameters;

        public void SimpleParameterTypeLists(List<string> stringValue,
            List<AttributeTargets> enumValue, List<char> charValue, List<bool> boolValue, List<sbyte> sbyteValue,
            List<byte> byteValue, List<short> shortValue, List<ushort> ushortValue, List<int> intValue, List<uint> uintValue,
            List<long> longValue, List<ulong> ulongValue, List<float> floatValue, List<double> doubleValue, List<decimal> decimalValue,
            List<DateTime> datetimeValue, List<Guid> guidValue, List<TimeSpan> tiemspanValue, List<Uri> uriValue,

            List<AttributeTargets?> enumNullableValue, List<char?> charNullableValue, List<bool?> boolNullableValue,
            List<sbyte?> sbyteNullableValue, List<byte?> byteNullableValue, List<short?> shortNullableValue,
            List<ushort?> ushortNullableValue, List<int?> intNullableValue, List<uint?> uintNullableValue,
            List<long?> longNullableValue, List<ulong?> ulongNullableValue, List<float?> floatNullableValue,
            List<double?> doubleNullableValue, List<decimal?> decimalNullableValue, List<DateTime?> datetimeNullableValue,
            List<Guid?> guidNullableValue, List<TimeSpan?> timespanNullableValue) { }

        [TestCaseSource(nameof(SimpleTypeParameterParsingTestCases))]
        public void Should_map_simple_types_to_a_list(Type type, object expected, object value)
        {
            var parameter = SimpleTypeParameterLists.FirstOrDefault(
                x => x.ParameterType.ElementType.Type == type);
            var result = new SimpleTypeMapper(new ParsedValueMapper()).Map(new ValueMapperContext(
                new ActionParameter(_actionMethod, parameter), new[] { value, value }));

            result.Status.ShouldEqual(MappingStatus.Success);
            result.Value?.GetType().ShouldEqual(typeof(List<>).MakeGenericType(type));
            result.Value.As<IEnumerable>().Cast<object>().ShouldOnlyContain(expected, expected);
        }

        private static readonly ParameterDescriptor[] SimpleTypeParameterListInterface =
            new TypeCache().GetTypeDescriptor(typeof(SimpleTypeMapperTests)).Methods
                .First(x => x.Name == nameof(SimpleParameterTypeListInterfaces)).Parameters;

        public void SimpleParameterTypeListInterfaces(IList<string> stringValue,
            IList<AttributeTargets> enumValue, IList<char> charValue, IList<bool> boolValue, IList<sbyte> sbyteValue,
            IList<byte> byteValue, IList<short> shortValue, IList<ushort> ushortValue, IList<int> intValue, IList<uint> uintValue,
            IList<long> longValue, IList<ulong> ulongValue, IList<float> floatValue, IList<double> doubleValue, IList<decimal> decimalValue,
            IList<DateTime> datetimeValue, IList<Guid> guidValue, IList<TimeSpan> tiemspanValue, IList<Uri> uriValue,

            IList<AttributeTargets?> enumNullableValue, IList<char?> charNullableValue, IList<bool?> boolNullableValue,
            IList<sbyte?> sbyteNullableValue, IList<byte?> byteNullableValue, IList<short?> shortNullableValue,
            IList<ushort?> ushortNullableValue, IList<int?> intNullableValue, IList<uint?> uintNullableValue,
            IList<long?> longNullableValue, IList<ulong?> ulongNullableValue, IList<float?> floatNullableValue,
            IList<double?> doubleNullableValue, IList<decimal?> decimalNullableValue, IList<DateTime?> datetimeNullableValue,
            IList<Guid?> guidNullableValue, IList<TimeSpan?> timespanNullableValue) { }

        [TestCaseSource(nameof(SimpleTypeParameterParsingTestCases))]
        public void Should_map_simple_types_to_a_list_interface(Type type, object expected, object value)
        {
            var parameter = SimpleTypeParameterListInterface.FirstOrDefault(
                x => x.ParameterType.ElementType.Type == type);
            var result = new SimpleTypeMapper(new ParsedValueMapper()).Map(new ValueMapperContext(
                new ActionParameter(_actionMethod, parameter), new[] { value, value }));

            result.Status.ShouldEqual(MappingStatus.Success);
            result.Value?.GetType().ShouldEqual(typeof(List<>).MakeGenericType(type));
            result.Value.As<IEnumerable>().Cast<object>().ShouldOnlyContain(expected, expected);
        }
    }
}
