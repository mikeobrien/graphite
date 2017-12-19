using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Graphite.Actions;
using Graphite.Binding;
using Graphite.Extensions;
using Graphite.Reflection;
using Graphite.Routing;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Binding
{
    [TestFixture]
    public class ParsedValueMapperTests
    {
        public class Model
        {
            public int Value { get; set; }
        }

        public class Handler
        {
            public void String(string value) { }
            public void StringArray(string[] value) { }
            public void StringList(IList<string> value) { }
            public void StringEnumerable(IEnumerable<string> value) { }

            public void Int(int value) { }
            public void IntArray(int[] value) { }
            public void IntList(IList<int> value) { }
            public void IntEnumerable(IEnumerable<int> value) { }

            public void Nullable(int value) { }
            public void NullableArray(int[] value) { }
            public void NullableList(IList<int> value) { }
            public void NullableEnumerable(IEnumerable<int> value) { }
            
            public void Property(Model value) { }
        }

        [Test]
        public void Should_map_single_string_value()
        {
            var result = MapString<Handler>(x => x.String(null), "fark");
            Should_return_value(result, "fark");
        }

        [Test]
        public void Should_map_single_null_string_value()
        {
            var result = MapString<Handler>(x => x.String(null), (object)null);
            Should_return_value<string>(result, null);
        }

        [Test]
        public void Should_map_multiple_to_single_string_value()
        {
            var result = MapString<Handler>(x => x.String(null), "fark", "farker");
            Should_return_value(result, "fark");
        }
        
        [Test]
        public void Should_map_string_array_value()
        {
            var result = MapString<Handler>(x => x.StringArray(null), "fark", "farker");
            Should_return_values<string[], string>(result, "fark", "farker");
        }
        
        [Test]
        public void Should_map_string_list_value()
        {
            var result = MapString<Handler>(x => x.StringList(null), "fark", "farker");
            Should_return_values<List<string>, string>(result, "fark", "farker");
        }
        
        [Test]
        public void Should_map_string_enumerable_value()
        {
            var result = MapString<Handler>(x => x.StringEnumerable(null), "fark", "farker");
            Should_return_values<List<string>, string>(result, "fark", "farker");
        }

        [Test]
        public void Should_map_single_int_value()
        {
            var result = MapInt<Handler>(x => x.Int(0), "5");
            Should_return_value(result, 5);
        }

        [Test]
        public void Should_map_multiple_to_single_int_value()
        {
            var result = MapInt<Handler>(x => x.Int(0), "5", "6");
            Should_return_value(result, 5);
        }
        
        [Test]
        public void Should_map_int_and_string_values()
        {
            var result = MapInt<Handler>(x => x.IntArray(null), 5, "6");
            Should_return_values<int[], int>(result, 5, 6);
        }
        
        [Test]
        public void Should_map_int_array_value()
        {
            var result = MapInt<Handler>(x => x.IntArray(null), "5", "6");
            Should_return_values<int[], int>(result, 5, 6);
        }
        
        [Test]
        public void Should_map_int_list_value()
        {
            var result = MapInt<Handler>(x => x.IntList(null), "5", "6");
            Should_return_values<List<int>, int>(result, 5, 6);
        }
        
        [Test]
        public void Should_map_int_enumerable_value()
        {
            var result = MapInt<Handler>(x => x.IntEnumerable(null), "5", "6");
            Should_return_values<List<int>, int>(result, 5, 6);
        }
        
        [Test]
        public void Should_map_single_nullable_value()
        {
            var result = MapNullableInt<Handler>(x => x.Nullable(0), "5");
            Should_return_value(result, 5);
        }
        
        [Test]
        public void Should_map_single_nullable_null_value()
        {
            var result = MapNullableInt<Handler>(x => x.Nullable(0), (object)null);
            Should_return_value<int?>(result, null);
        }

        [Test]
        public void Should_map_multiple_to_single_nullable_value()
        {
            var result = MapNullableInt<Handler>(x => x.Nullable(0), "5", "6");
            Should_return_value(result, 5);
        }
        
        [Test]
        public void Should_map_nullable_array_value()
        {
            var result = MapNullableInt<Handler>(x => x.NullableArray(null), "5", "6");
            Should_return_values<int?[], int?>(result, 5, 6);
        }
        
        [Test]
        public void Should_map_nullable_list_value()
        {
            var result = MapNullableInt<Handler>(x => x.NullableList(null), "5", "6");
            Should_return_values<List<int?>, int?>(result, 5, 6);
        }
        
        [Test]
        public void Should_fail_to_cast_value()
        {
            var result = MapInt<Handler>(x => x.IntList(null), 5.5);

            result.Status.ShouldEqual(MappingStatus.Failure);
            result.ErrorMessage.ShouldEqual("Parameter 'value' value '5.5' is not formatted correctly. " +
                                            "Could not cast type System.Double to System.Int32.");
            result.Value.ShouldBeNull();
        }
        
        [Test]
        public void Should_return_parameter_parse_failure()
        {
            var result = MapInt<Handler>(x => x.IntList(null), "5.5");

            result.Status.ShouldEqual(MappingStatus.Failure);
            result.ErrorMessage.ShouldEqual(
                "Parameter 'value' value '5.5' is not formatted correctly. '5.5' is not a valid 32 bit integer. " +
                "Must be an integer between -2,147,483,648 and 2,147,483,647.");
            result.Value.ShouldBeNull();
        }
        
        [Test]
        public void Should_return_parameter_property_parse_failure()
        {
            var result = new ParsedValueMapper().Map(new ValueMapperContext(
                GetActionParameterProperty<Handler>(x => x.Property(null)), 
                new object[] { "5.5" }), x => x.TryParseNullableInt32());

            result.Status.ShouldEqual(MappingStatus.Failure);
            result.ErrorMessage.ShouldEqual(
                "Parameter 'value.Value' value '5.5' is not formatted correctly. " +
                "'5.5' is not a valid 32 bit integer. Must be an integer between -2,147,483,648 and 2,147,483,647.");
            result.Value.ShouldBeNull();
        }
        
        [Test]
        public void Should_return_property_parse_failure()
        {
            var result = new ParsedValueMapper().Map(new ValueMapperContext(
                GetActionProperty<Handler, Model>(x => x.Property(null)), 
                new object[] { "5.5" }), x => x.TryParseNullableInt32());

            result.Status.ShouldEqual(MappingStatus.Failure);
            result.ErrorMessage.ShouldEqual(
                "Property 'Value' value '5.5' is not formatted correctly. " +
                "'5.5' is not a valid 32 bit integer. Must be an integer between -2,147,483,648 and 2,147,483,647.");
            result.Value.ShouldBeNull();
        }

        public void Should_return_value<T>(MapResult result, T value)
        {
            result.Status.ShouldEqual(MappingStatus.Success);
            if (value != null) result.Value.ShouldBeType<T>();
            result.Value.ShouldEqual(value);
            result.ErrorMessage.ShouldBeNull();
        }

        public void Should_return_values<T, TValue>(MapResult result, params TValue[] values)
        {
            result.Status.ShouldEqual(MappingStatus.Success);
            result.Value.ShouldBeType<T>();
            result.Value.As<IEnumerable<TValue>>().ShouldOnlyContain(values);
            result.ErrorMessage.ShouldBeNull();
        }
        
        private MapResult MapString<T>(Expression<Action<T>> actionMethod, params object[] values)
        {
            return Map(actionMethod, x => ParseResult<string>.Succeeded(x, x), values);
        }
        
        private MapResult MapInt<T>(Expression<Action<T>> actionMethod, params object[] values)
        {
            return Map(actionMethod, x => x.TryParseInt32(), values);
        }
        
        private MapResult MapNullableInt<T>(Expression<Action<T>> actionMethod, params object[] values)
        {
            return Map(actionMethod, x => x.TryParseNullableInt32(), values);
        }
        
        private MapResult Map<T, TValue>(Expression<Action<T>> actionMethod, 
            Func<string, ParseResult<TValue>> parse, params object[] values)
        {
            return new ParsedValueMapper().Map(new ValueMapperContext(
                GetActionParameter(actionMethod), values), parse);
        }

        private ActionParameter GetActionParameter<T>(Expression<Action<T>> actionMethod)
        {
            var action = ActionMethod.From(actionMethod);
            var parameter = action.MethodDescriptor.Parameters.FirstOrDefault();
            return new ActionParameter(action, new ParameterDescriptor(
                new TypeCache().GetTypeDescriptor(parameter.ParameterType.Type),
                action.MethodDescriptor, parameter.ParameterInfo, new TypeCache()));
        }

        private ActionParameter GetActionParameterProperty<T>(Expression<Action<T>> actionMethod)
        {
            var typeCache = new TypeCache();
            var action = ActionMethod.From(actionMethod);
            var parameter = action.MethodDescriptor.Parameters.FirstOrDefault();
            var property = parameter.ParameterType.Properties.FirstOrDefault();
            return new ActionParameter(action, new ParameterDescriptor(
                    typeCache.GetTypeDescriptor(parameter.ParameterType.Type),
                action.MethodDescriptor, parameter.ParameterInfo, typeCache),
                new PropertyDescriptor(property.PropertyType, property.PropertyInfo, typeCache));
        }

        private ActionParameter GetActionProperty<T, TProperty>(Expression<Action<T>> actionMethod)
        {
            var typeCache = new TypeCache();
            var action = ActionMethod.From(actionMethod);
            var property = typeCache.GetTypeDescriptor(typeof(TProperty)).Properties.FirstOrDefault();
            return new ActionParameter(action, new PropertyDescriptor(property.PropertyType, 
                property.PropertyInfo, typeCache));
        }
    }
}
