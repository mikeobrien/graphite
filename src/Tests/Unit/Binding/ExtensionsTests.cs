using System.Collections.Generic;
using System.Linq;
using Graphite;
using Graphite.Binding;
using NUnit.Framework;
using Should;
using Tests.Common.Fakes;

namespace Tests.Unit.Binding
{
    [TestFixture]
    public class ExtensionsTests
    {
        [Test]
        public void Should_map_parameter_with_first_matching_mapper_applies_to()
        {
            var mappers = new List<IValueMapper>
            {
                new TestValueMapper { MapFunc = c => $"{c.Values.First()}1", AppliesToFunc = c => false },
                new TestValueMapper { MapFunc = c => $"{c.Values.First()}2" }
            };

            var result = mappers.Map(new object[] { "value" }, null, null, new Configuration());

            result.Mapped.ShouldBeTrue();
            result.Value.ShouldEqual("value2");
        }

        [Test]
        public void Should_map_parameter_with_first_matching_configuration_applies_to()
        {
            var mappers = new List<IValueMapper>
            {
                new TestValueMapper1 { MapFunc = c => $"{c.Values.First()}1" },
                new TestValueMapper2 { MapFunc = c => $"{c.Values.First()}2" }
            };
            var configuration = new Configuration();

            configuration.ValueMappers.Append<TestValueMapper1>(x => false);

            var result = mappers.Map(new object[] { "value" }, null, null, configuration);

            result.Mapped.ShouldBeTrue();
            result.Value.ShouldEqual("value2");
        }

        [Test]
        public void Should_return_original_parameter_value_if_mapper_not_found()
        {
            var mappers = new List<IValueMapper>
            {
                new TestValueMapper { MapFunc = c => $"{c.Values.First()}1", AppliesToFunc = c => false }
            };

            var result = mappers.Map(new object[] { "value" }, 
                null, null, new Configuration());

            result.Mapped.ShouldBeFalse();
            result.Value.ShouldBeNull();
        }

        [Test]
        public void Should_map_property_with_first_matching_configuration_applies_to()
        {
            var mappers = new List<IValueMapper>
            {
                new TestValueMapper1 { MapFunc = c => $"{c.Values.First()}1" },
                new TestValueMapper2 { MapFunc = c => $"{c.Values.First()}2" }
            };
            var configuration = new Configuration();

            configuration.ValueMappers.Append<TestValueMapper1>(x => false);

            var result = mappers.Map(new object[] { "value" }, null, null, configuration);

            result.Mapped.ShouldBeTrue();
            result.Value.ShouldEqual("value2");
        }

        [Test]
        public void Should_return_original_property_value_if_mapper_not_found()
        {
            var mappers = new List<IValueMapper>
            {
                new TestValueMapper { MapFunc = c => $"{c.Values.First()}1", AppliesToFunc = c => false }
            };

            var result = mappers.Map(new object[] { "value" }, null, null, new Configuration());

            result.Mapped.ShouldBeFalse();
            result.Value.ShouldBeNull();
        }
    }
}
