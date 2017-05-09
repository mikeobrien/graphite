using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Graphite.Binding;
using Graphite.Extensions;
using Graphite.Reflection;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Binding
{
    [TestFixture]
    public class ValueMapperBaseTests
    {
        public class TestValueMapper : ValueMapperBase
        {
            public override bool AppliesTo(ValueMapperContext context)
            {
                throw new NotImplementedException();
            }

            public override object Map<T>(ValueMapperContext context)
            {
                if (typeof(T) != typeof(int)) return default(T);
                var result = context.Values.SelectMany(x => x.ToString().Split(','))
                    .Select(x => (T)(object)int.Parse(x.ToString()));
                return context.Type.IsArray 
                    ? result.ToArray() 
                    : (context.Type.IsGenericListCastable 
                        ? result.ToList() 
                        : (object)result.FirstOrDefault());
            }
        }

        public class Model
        {
            public int Value { get; set; }
            public int[] Array { get; set; }
            public List<int> List { get; set; }
        }

        private static readonly Dictionary<string, PropertyDescriptor> Properties =
            new TypeCache().GetTypeDescriptor(typeof(Model)).Properties.ToDictionary(x => x.Name, x => x);

        [Test]
        public void Should_map_source_to_value()
        {
            var result = new TestValueMapper().Map(new ValueMapperContext(null,
                 null, Properties[nameof(Model.Value)], new object[] { "1,2,3" }));
            result.ShouldBeType<int>();
            result.CastTo<int>().ShouldEqual(1);
        }

        [Test]
        public void Should_map_source_to_array()
        {
            var result = new TestValueMapper().Map(new ValueMapperContext(null,
                 null, Properties[nameof(Model.Array)], new object[] { "1,2,3" }));
            result.ShouldBeType<int[]>();
            result.CastTo<int[]>().ShouldOnlyContain(1, 2, 3);
        }

        [Test]
        public void Should_map_source_to_list()
        {
            var result = new TestValueMapper().Map(new ValueMapperContext(null,
                 null, Properties[nameof(Model.List)], new object[] { "1,2,3" }));
            result.ShouldBeType<List<int>>();
            result.CastTo<List<int>>().ShouldOnlyContain(1, 2, 3);
        }

        [Test]
        public void Should_set_properties_faster_than_reflection()
        {
            var iterations = 10000;
            var mapper = new TestValueMapper();
            var mapperContext = new ValueMapperContext(null, null,
                Properties[nameof(Model.Value)], new object[] {"1,2,3"});
            var method = typeof(TestValueMapper)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .First(x => x.Name == nameof(TestValueMapper.Map) && x.IsGenericMethod)
                .MakeGenericMethod(mapperContext.Type.Type);

            var nativeElapsed = new List<long>();
            var reflectionElapsed = new List<long>();
            var expressionElapsed = new List<long>();

            Action run = () =>
            {
                expressionElapsed.Add(mapper.ElapsedTicks(x => x.Map(mapperContext)));
                reflectionElapsed.Add(method.ElapsedTicks(x =>
                {
                    x.Invoke(mapper, new object[] { mapperContext });

                }));
                nativeElapsed.Add(mapper.ElapsedTicks(x =>
                {
                    x.Map<int>(mapperContext);
                }));
            };

            100.Times(run); // Warmup

            nativeElapsed.Clear();
            reflectionElapsed.Clear();
            expressionElapsed.Clear();

            iterations.Times(run);

            Console.WriteLine($"Native:              {nativeElapsed.Average()}");
            Console.WriteLine($"Compiled expression: {expressionElapsed.Average()}");
            Console.WriteLine($"Reflection:          {reflectionElapsed.Average()}");
        }
    }
}
