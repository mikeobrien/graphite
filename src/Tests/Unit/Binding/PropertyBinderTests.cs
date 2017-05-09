using System;
using System.Collections.Generic;
using System.Linq;
using Graphite.Binding;
using Graphite.Extensions;
using Graphite.Reflection;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Binding
{
    [TestFixture]
    public class PropertyBinderTests
    {
        public class HydratedObject
        {
            public object[] Value1 { get; set; }
            public object[] Value2 { get; set; }
        }

        [Test]
        public void Should_set_properties_on_an_existing_object()
        {
            var instance = new TypeCache().GetTypeDescriptor(typeof(HydratedObject))
                .CreateAndBind(new List<string, object>
            {
                { "value0", "farker" },
                { "value1", "fark" },
                { "value2", "valueA" },
                { "value2", "valueB" }
            }.ToLookup(), (p, o) => MapResult.WasMapped(o)) as HydratedObject;

            instance.Value1.ShouldOnlyContain("fark");
            instance.Value2.ShouldOnlyContain("valueA", "valueB");
        }

        [Test]
        public void Should_conditionally_set_properties()
        {
            var instance = new TypeCache().GetTypeDescriptor(typeof(HydratedObject))
                .CreateAndBind(new List<string, object>
            {
                { "value0", "farker" },
                { "value1", "fark" },
                { "value2", "valueA" },
                { "value2", "valueB" }
            }.ToLookup(), (p, o) => p.Name == "Value2" ? 
                MapResult.WasMapped(o) : MapResult.NotMapped()) as HydratedObject;

            instance.Value1.ShouldBeNull();
            instance.Value2.ShouldOnlyContain("valueA", "valueB");
        }

        [Test]
        public void Should_set_properties_faster_than_reflection()
        {
            var iterations = 10000;
            var type = new TypeCache().GetTypeDescriptor(typeof(HydratedObject));
            var values = new List<string, object>
            {
                { "value0", "farker" },
                { "value1", "fark" },
                { "value2", "valueA" },
                { "value2", "valueB" }
            }.ToLookup();
            Func<PropertyDescriptor, object[], MapResult> map = (p,  o) => MapResult.WasMapped(o);

            var nativeElapsed = new List<long>();
            var reflectionElapsed = new List<long>();
            var expressionElapsed = new List<long>();

            Action run = () =>
            {
                expressionElapsed.Add(type.ElapsedTicks(x => x.CreateAndBind(values, map)));
                reflectionElapsed.Add(values.ElapsedTicks(x =>
                {
                    values.ForEach(v =>
                    {
                        var instance = Activator.CreateInstance(typeof(HydratedObject));
                        var property = type.Properties.FirstOrDefault(p => p.Name
                            .Equals(v.Key, StringComparison.OrdinalIgnoreCase));
                        if (property != null)
                        {
                            var result = map(property, v.ToArray());
                            if (result.Mapped) property.PropertyInfo.SetValue(instance, result.Value);
                        }
                    });
                }));
                nativeElapsed.Add(values.ElapsedTicks(x =>
                {
                    var instance = new HydratedObject();
                    foreach (var value in values)
                    {
                        if (value.Key.Equals("value1", StringComparison.OrdinalIgnoreCase))
                        {
                            var result = map(null, value.ToArray());
                            if (result.Mapped) instance.Value1 = (object[])result.Value;
                        }
                        else if (value.Key.Equals("value2", StringComparison.OrdinalIgnoreCase))
                        {
                            var result = map(null, value.ToArray());
                            if (result.Mapped) instance.Value2 = (object[])result.Value;
                        }
                    }
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
